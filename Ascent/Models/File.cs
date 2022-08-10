using System.ComponentModel.DataAnnotations;

namespace Ascent.Models;

public class File
{
    public int Id { get; set; }

    [Required, MaxLength(1000)]
    public string Name { get; set; }

    public int Version { get; set; } = 1;

    [MaxLength(255)]
    public string ContentType { get; set; }

    public long Size { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;

    public int? ParentId { get; set; }
    public File Parent { get; set; }

    public List<File> Children { get; set; }

    public bool IsFolder { get; set; }
    public bool IsPublic { get; set; }

    // Files created via the File UI are considered "regular", while files uploaded as part of
    // some other resources (like a Course or CourseJournal) are considered non-regular.
    // Certain file functions (e.g. search, public, etc.) are limited to regular files.
    public bool IsRegular { get; set; }
}

public class FileRevision
{
    public int FileId { get; set; }
    public File File { get; set; }

    [Required, MaxLength(1000)]
    public string Name { get; set; }

    public int Version { get; set; }

    [MaxLength(255)]
    public string ContentType { get; set; }

    public long Size { get; set; }

    public DateTime TimeCreated { get; set; }
}
