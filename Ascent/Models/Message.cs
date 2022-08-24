using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Models;

[Owned]
public class Author
{
    [Required, MaxLength(255)]
    public string Id { get; set; }
    [Required, MaxLength(255)]
    public string FirstName { get; set; }
    [Required, MaxLength(255)]
    public string LastName { get; set; }
    [Required, MaxLength(255)]
    public string Email { get; set; }

    public string Name => $"{FirstName} {LastName}";
}

public class Message
{
    public int Id { get; set; }

    [Required]
    public Author Author { get; set; }

    // For a group recipient we record the group name, and for an individual
    // recipient we record the email address.
    [Required, MaxLength(255)]
    public string Recipient { get; set; }

    [Required, MaxLength(255)]
    public string Subject { get; set; }

    [Required]
    public string Content { get; set; }

    public bool UseBcc { get; set; }

    public DateTime TimeSent { get; set; }
    public bool IsFailed { get; set; } // Whether the email was sent successfully
}
