using System.ComponentModel.DataAnnotations;

namespace Ascent.Models;

public class Page
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Subject { get; set; }

    public string Content { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    public DateTime? Published { get; set; }
    public bool IsPublic => Published != null && Published < DateTime.UtcNow;

    public int ViewCount { get; set; }

    public bool Deleted { get; set; }
}
