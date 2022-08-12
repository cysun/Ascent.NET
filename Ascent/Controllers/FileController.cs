using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class FileController : Controller
    {
        private readonly FileService _fileService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<FileController> _logger;

        public FileController(FileService fileService, IAuthorizationService authorizationService,
            IMapper mapper, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(string searchText)
        {
            List<Models.File> files;

            if (string.IsNullOrWhiteSpace(searchText))
                files = _fileService.GetFiles();
            else
                files = _fileService.SearchFiles(searchText);

            return View(files);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();

            ViewBag.File = file;
            ViewBag.Ancestors = _fileService.GetAncestors(file);

            return View(_mapper.Map<FileInputModel>(file));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, FileInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();

            _mapper.Map(input, file);
            _fileService.SaveChanges();
            _logger.LogInformation("User {user} edited {file}", User.Identity.Name, file.Id);

            if (file.ParentId != null)
                return RedirectToAction("View", "Folder", new { id = file.ParentId });
            else
                return RedirectToAction("Index");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Move(int id, int? parentId)
        {
            if (id == parentId) return BadRequest();

            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();

            var parent = parentId != null ? _fileService.GetFile((int)parentId) : null;
            if (parent != null && !parent.IsFolder)
                return BadRequest();

            file.ParentId = parentId;
            _fileService.SaveChanges();
            _logger.LogInformation("User {user} moved {file} from {oldParent} to {newParent}",
                User.Identity.Name, file.Id, file.ParentId, parentId);

            if (parentId != null)
                return RedirectToAction("View", "Folder", new { id = file.ParentId });
            else
                return RedirectToAction("Index");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();

            if (file.IsFolder)
            {
                var filesDeleted = _fileService.DeleteFolder(id);
                _logger.LogInformation("User {user} deleted folder {folder} with {n} files",
                    User.Identity.Name, file.Name, filesDeleted);
            }
            else
            {
                var versionsDeleted = _fileService.DeleteFile(id);
                _logger.LogInformation("User {user} deleted file {file} with {n} versions",
                    User.Identity.Name, file.Name, versionsDeleted);
            }

            if (file.ParentId != null)
                return RedirectToAction("View", "Folder", new { id = file.ParentId });
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Upload(int? parentId, IFormFile[] uploadedFiles)
        {
            foreach (var uploadedFile in uploadedFiles)
                _fileService.UploadFile(parentId, uploadedFile);

            return Ok();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ViewAsync(int id)
        {
            return await DownloadAsync(id, true);
        }

        [AllowAnonymous]
        public async Task<IActionResult> DownloadAsync(int id, bool inline = false)
        {
            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();
            if (file.IsFolder) return BadRequest();

            if (!file.IsPublic && !(await _authorizationService.AuthorizeAsync(User, Constants.Policy.CanRead)).Succeeded)
                return Forbid();

            var diskFile = _fileService.GetDiskFile(file.Id, file.Version);

            inline = inline && !_fileService.IsAttachmentType(file.Name);
            return !inline ? PhysicalFile(diskFile, file.ContentType, file.Name) :
                PhysicalFile(diskFile, _fileService.IsTextType(file.Name) ? "text/plain" : file.ContentType);
        }

        [HttpPut("file/{id}/{field}")]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult SetField(int id, [Required] string field, string value)
        {
            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();

            switch (field.ToLower())
            {
                case "ispublic":
                    file.IsPublic = bool.Parse(value);
                    break;
                default:
                    _logger.LogWarning("Unrecognized field: {field}", field);
                    return BadRequest();
            }

            _fileService.SaveChanges();
            _logger.LogInformation("{user} set file {file} field {field} to {value}", User.Identity.Name,
                file.Id, field, value);

            return Ok();
        }

        public List<Models.File> Autocomplete(string searchText)
        {
            return _fileService.SearchFiles(searchText, 10).Where(f => !f.IsFolder).ToList();
        }
    }
}

namespace Ascent.Models
{
    public class FileInputModel
    {
        [Required]
        [MaxLength(1000)]
        public string Name { get; set; }

        [Display(Name = "Public")]
        public bool IsPublic { get; set; }
    }
}
