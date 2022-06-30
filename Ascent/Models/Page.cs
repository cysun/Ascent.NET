using System.ComponentModel.DataAnnotations;

namespace Ascent.Models;

public class Page
{
    public int Id { get; set; }

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

public class PageHistory
{
    public int PageId { get; set; }
    public Page Page { get; set; }

    [Required]
    [MaxLength(80)]
    public string Subject { get; set; }

    public string Content { get; set; }

    public DateTime TimeCreated { get; set; } // time when the revision is created
}
