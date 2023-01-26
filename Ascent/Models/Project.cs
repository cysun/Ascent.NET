using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Models;

public class Project
{
    public int Id { get; set; }

    // In the form of xxxx-yyyy, e.g. 2022-2023
    [MaxLength(12)]
    public string AcademicYear { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; }

    public string Description { get; set; }

    [MaxLength(255)]
    public string Sponsor { get; set; }

    public List<ProjectStudent> Students { get; set; } = new List<ProjectStudent>();
    public List<ProjectAdvisor> Advisors { get; set; } = new List<ProjectAdvisor>();
    public List<ProjectLiaison> Liaisons { get; set; } = new List<ProjectLiaison>();

    public List<ProjectItem> Items { get; set; } = new List<ProjectItem>();
}

[PrimaryKey(nameof(ProjectId), nameof(PersonId))]
public class ProjectStudent
{
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }
}

[PrimaryKey(nameof(ProjectId), nameof(PersonId))]
public class ProjectAdvisor
{
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }
}

[PrimaryKey(nameof(ProjectId), nameof(PersonId))]
public class ProjectLiaison
{
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }
}

public class ProjectItem
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(10)]
    public ItemType Type { get; set; }

    public int? FileId { get; set; }
    public File File { get; set; }

    public string Text { get; set; }

    [MaxLength(2000)]
    public string Url { get; set; }

    public bool IsPrivate { get; set; }
}
