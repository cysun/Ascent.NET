using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Ascent.Services;

public class FilesSettings
{
    public string Directory { get; set; }

    // Attachment Types are files (e.g. doc) whose Content-Disposition header should be
    // set to attachment even for View File operation. These files cannot be displayed
    // directly in browser so browsers will try to save them. Providing a file name to
    // PhysicalFile() will ensure that the file is saved with the right name instead of
    // having id as its name.
    public HashSet<string> AttachmentTypes { get; set; }

    // Text Types are files (e.g. java) that should be displayed directly in browser.
    // Browsers may not display them because of their content types, so we'll overwrite
    // their content types with "text/plain".
    public HashSet<string> TextTypes { get; set; }
}

public class FileService
{
    private readonly AppDbContext _db;

    private readonly IMapper _mapper;
    private readonly ILogger<FileService> _logger;

    private readonly FilesSettings _settings;

    public FileService(AppDbContext db, IMapper mapper, ILogger<FileService> logger, IOptions<FilesSettings> settings)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
        _settings = settings.Value;
    }

    public bool IsAttachmentType(string fileName) => _settings.AttachmentTypes.Contains(Path.GetExtension(fileName).ToLower());

    public bool IsTextType(string fileName) => _settings.TextTypes.Contains(Path.GetExtension(fileName).ToLower());

    public List<Models.File> GetFiles() => _db.Files.AsNoTracking()
        .Where(f => f.ParentId == null && f.IsRegular)
        .OrderByDescending(f => f.IsFolder).ThenBy(f => f.Name)
        .ToList();

    public Models.File GetFile(int id) => _db.Files.Find(id);

    public Models.File GetFolder(int id) => _db.Files.AsNoTracking()
        .Where(f => f.Id == id && f.IsFolder)
        .Include(f => f.Children.OrderByDescending(c => c.IsFolder).ThenBy(c => c.Name))
        .SingleOrDefault();

    public Models.File GetFolder(string path, bool createFolder = true, bool isRegular = true)
    {
        string[] names = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Models.File parent = null;
        foreach (var name in names)
        {
            var folder = GetFile(parent?.Id, name);
            if (folder == null)
            {
                if (!createFolder) return null;
                folder = new Models.File()
                {
                    Name = name,
                    IsFolder = true,
                    IsRegular = isRegular,
                    ParentId = parent?.Id
                };
                _db.Files.Add(folder);
                _db.SaveChanges();
                _logger.LogInformation("New folder created: {folder}", folder.Id);
            }
            parent = folder;
        }
        return parent;
    }

    public List<Models.File> GetAncestors(Models.File file)
    {
        var ancestors = new List<Models.File>();

        var parentId = file.ParentId;
        while (parentId != null)
        {
            var parent = _db.Files.Find(parentId);
            ancestors.Insert(0, parent);
            parentId = parent.ParentId;
        }

        return ancestors;
    }

    public List<Models.File> GetChildren(int? parentId, bool folderOnly = false)
    {
        return _db.Files.Where(f => f.ParentId == parentId && (f.IsFolder || !folderOnly))
            .OrderByDescending(f => f.IsFolder).ThenBy(f => f.Name)
            .AsNoTracking().ToList();
    }

    public Models.File GetFile(int? parentId, string name)
    {
        return _db.Files.Where(f => f.ParentId == parentId && f.Name == name).FirstOrDefault();
    }

    public Models.File UploadFile(int? parentId, IFormFile uploadedFile, bool IsRegular = true, string fileName = null)
    {
        string name = Path.GetFileName(uploadedFile.FileName);
        if (fileName != null)
            name = fileName + Path.GetExtension(name);

        var file = GetFile(parentId, name);
        if (file == null)
        {
            file = new Models.File
            {
                Name = name,
                ContentType = uploadedFile.ContentType,
                Size = uploadedFile.Length,
                ParentId = parentId,
                IsRegular = IsRegular
            };
            if (parentId != null)
            {
                var parent = GetFile((int)parentId);
                file.IsPublic = parent.IsPublic;
            }
            _db.Files.Add(file);
            _logger.LogDebug("New file uploaded: {file}", name);
        }
        else
        {
            _db.FileRevisions.Add(_mapper.Map<Models.FileRevision>(file));
            file.ContentType = uploadedFile.ContentType;
            file.Size = uploadedFile.Length;
            file.TimeCreated = DateTime.UtcNow;
            file.Version++;
            _logger.LogDebug("Existing file updated: {file}", name);
        }
        _db.SaveChanges();
        _logger.LogInformation("File saved to database: {file}", name);

        string diskFile = Path.Combine(_settings.Directory, $"{file.Id}-{file.Version}");
        using (var fileStream = new FileStream(diskFile, FileMode.Create))
        {
            uploadedFile.CopyTo(fileStream);
        }
        _logger.LogInformation("File saved to disk: {file}", name);

        return file;
    }

    public string GetDiskFile(int fileId, int version)
    {
        return Path.Combine(_settings.Directory, $"{fileId}-{version}");
    }

    public void AddFolder(Models.File folder) => _db.Files.Add(folder);

    // maxResults=null for unlimited results
    public List<Models.File> SearchFiles(string searchText, int? maxResults = 20)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return new List<Models.File>();

        return _db.Files.FromSqlRaw("SELECT * FROM \"SearchFiles\"({0}, {1})", searchText, maxResults)
            .OrderByDescending(f => f.IsFolder).ThenBy(f => f.Name)
            .AsNoTracking().ToList();
    }

    public int DeleteFile(int id)
    {
        var file = _db.Files.Find(id);
        if (file != null)
        {
            for (int i = 1; i < file.Version; ++i)
            {
                File.Delete(GetDiskFile(id, i));
                _db.FileRevisions.Remove(new Models.FileRevision
                {
                    FileId = id,
                    Version = i
                });
            }
        }
        File.Delete(GetDiskFile(id, file.Version));
        _db.Files.Remove(file);
        _db.SaveChanges();

        return file.Version;
    }

    public int DeleteFolder(int id)
    {
        var totalRemoved = 0;
        var folder = _db.Files.Find(id);
        if (folder != null)
        {
            var children = _db.Files.Where(f => f.ParentId == id).ToList();
            foreach (var child in children)
            {
                if (child.IsFolder)
                    totalRemoved += DeleteFolder(child.Id);
                else
                {
                    DeleteFile(child.Id);
                    totalRemoved++;
                }
            }

            _db.Files.Remove(folder);
            _db.SaveChanges();
            ++totalRemoved;
        }

        return totalRemoved;
    }

    public void SaveChanges() => _db.SaveChanges();
}
