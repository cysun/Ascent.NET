using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

[Table("projects")]
public class Project
{
    [Column("id")]
    public long Id { get; set; }

    [Column("title")]
    public string Title { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("year")]
    public int Year { get; set; }

    [Column("published")]
    public bool Published { get; set; }

    [Column("deleted")]
    public bool Deleted { get; set; }

    [Column("sponsor")]
    public string Sponsor { get; set; }

    [Column("private")]
    public bool Private { get; set; }

    public List<ProjectStudent> Students { get; set; } = new List<ProjectStudent>();
    public List<ProjectAdvisor> Advisors { get; set; } = new List<ProjectAdvisor>();
    public List<ProjectLiaison> Liaisons { get; set; } = new List<ProjectLiaison>();
    public List<ProjectResource> Resources { get; set; } = new List<ProjectResource>();
}

[Table("project_students")]
public class ProjectStudent
{
    [Column("project_id")]
    public long ProjectId { get; set; }
    public Project Project { get; set; }

    [Column("student_id")]
    public long UserId { get; set; }
    public User User { get; set; }
}

[Table("project_advisors")]
public class ProjectAdvisor
{
    [Column("project_id")]
    public long ProjectId { get; set; }
    public Project Project { get; set; }

    [Column("advisor_id")]
    public long UserId { get; set; }
    public User User { get; set; }
}

[Table("project_liaisons")]
public class ProjectLiaison
{
    [Column("project_id")]
    public long ProjectId { get; set; }
    public Project Project { get; set; }

    [Column("liaison_id")]
    public long UserId { get; set; }
    public User User { get; set; }
}

[Table("project_resources")]
public class ProjectResource
{
    [Column("project_id")]
    public long ProjectId { get; set; }
    public Project Project { get; set; }

    [Column("resource_id")]
    public long ResourceId { get; set; }
    public Resource Resource { get; set; }

    [Column("resource_order")]
    public long ResourceOrder { get; set; }
}
