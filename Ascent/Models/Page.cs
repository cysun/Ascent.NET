using System.ComponentModel.DataAnnotations;

namespace Ascent.Models;

public class Page
{
    public int Id { get; set; }

    public int Version { get; set; } = 1;

    [Required]
    [MaxLength(80)]
    public string Subject { get; set; }

    public string Content { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime TimeUpdated { get; set; } = DateTime.UtcNow;
    public DateTime TimeViewed { get; set; } = DateTime.UtcNow;

    public bool IsPublic { get; set; }
    public bool IsDeleted { get; set; }
}

public class PageRevision
{
    public int PageId { get; set; }
    public Page Page { get; set; }

    public int Version { get; set; }

    [Required]
    [MaxLength(80)]
    public string Subject { get; set; }

    public string Content { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.UtcNow; // time when the revision is created
}

// It's PageRevision without Content, so when we display a list of revisions, we
// don't have to load all the content in memory.
public class PageRevisionDto
{
    public int PageId { get; set; }

    public int Version { get; set; }

    public string Subject { get; set; }

    public DateTime TimeCreated { get; set; }
}
