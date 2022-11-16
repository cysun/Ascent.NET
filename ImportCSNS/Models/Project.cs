namespace ImportCSNS.Models;

public partial class Project
{
    public long Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public long? DepartmentId { get; set; }

    public int Year { get; set; }

    public bool Published { get; set; }

    public bool Deleted { get; set; }

    public string Sponsor { get; set; }

    public bool Private { get; set; }

    public virtual ICollection<ProjectResource> ProjectResources { get; } = new List<ProjectResource>();

    public virtual ICollection<User> Advisors { get; } = new List<User>();

    public virtual ICollection<User> Liaisons { get; } = new List<User>();

    public virtual ICollection<User> Students { get; } = new List<User>();
}
