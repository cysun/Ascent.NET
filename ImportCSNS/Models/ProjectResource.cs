namespace ImportCSNS.Models;

public partial class ProjectResource
{
    public long ProjectId { get; set; }

    public long ResourceId { get; set; }

    public long ResourceOrder { get; set; }

    public virtual Project Project { get; set; }

    public virtual Resource Resource { get; set; }
}
