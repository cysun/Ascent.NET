namespace ImportCSNS.Models;

public partial class User
{
    public long Id { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool? Enabled { get; set; }

    public string Cin { get; set; }

    public string LastName { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string PrimaryEmail { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string Zip { get; set; }

    public string HomePhone { get; set; }

    public int NumOfForumPosts { get; set; }

    public string SecondaryEmail { get; set; }

    public int DiskQuota { get; set; }

    public string WorkPhone { get; set; }

    public string CellPhone { get; set; }

    public bool Temporary { get; set; }

    public string AccessKey { get; set; }

    public long? OriginalPictureId { get; set; }

    public long? ProfilePictureId { get; set; }

    public long? ProfileThumbnailId { get; set; }

    public long? MajorId { get; set; }

    public long? PersonalProgramId { get; set; }

    public virtual ICollection<File> Files { get; } = new List<File>();

    public virtual File OriginalPicture { get; set; }

    public virtual File ProfilePicture { get; set; }

    public virtual File ProfileThumbnail { get; set; }

    public virtual ICollection<Project> Projects { get; } = new List<Project>();

    public virtual ICollection<Project> Projects1 { get; } = new List<Project>();

    public virtual ICollection<Project> ProjectsNavigation { get; } = new List<Project>();
}
