namespace ImportCSNS.Models;

public partial class Resource
{
    public long Id { get; set; }

    public string Name { get; set; }

    public int Type { get; set; }

    public string Text { get; set; }

    public long? FileId { get; set; }

    public string Url { get; set; }

    public bool Private { get; set; }

    public bool Deleted { get; set; }

    public virtual File File { get; set; }

    public virtual ICollection<ProjectResource> ProjectResources { get; } = new List<ProjectResource>();
}
