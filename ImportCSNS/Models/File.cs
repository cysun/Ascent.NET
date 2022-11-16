namespace ImportCSNS.Models;

public partial class File
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public long? Size { get; set; }

    public DateTime? Date { get; set; }

    public long OwnerId { get; set; }

    public bool Public { get; set; }

    public bool Folder { get; set; }

    public long? ParentId { get; set; }

    public bool Regular { get; set; }

    public bool Deleted { get; set; }

    public long? SubmissionId { get; set; }

    public long? ReferenceId { get; set; }

    public virtual ICollection<File> InverseParent { get; } = new List<File>();

    public virtual ICollection<File> InverseReference { get; } = new List<File>();

    public virtual User Owner { get; set; }

    public virtual File Parent { get; set; }

    public virtual File Reference { get; set; }

    public virtual ICollection<Resource> Resources { get; } = new List<Resource>();

    public virtual ICollection<User> UserOriginalPictures { get; } = new List<User>();

    public virtual ICollection<User> UserProfilePictures { get; } = new List<User>();

    public virtual ICollection<User> UserProfileThumbnails { get; } = new List<User>();
}
