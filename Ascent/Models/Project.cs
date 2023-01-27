using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Models;

public class Project
{
    public enum MemberType
    {
        Student = 1,
        Advisor = 2,
        Liaison = 3
    }

    public int Id { get; set; }

    // In the form of xxxx-yyyy, e.g. 2022-2023
    [MaxLength(12)]
    public string AcademicYear { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; }

    public string Description { get; set; }

    [MaxLength(255)]
    public string Sponsor { get; set; }

    public List<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public List<ProjectResource> Items { get; set; } = new List<ProjectResource>();

    public bool IsDeleted { get; set; }

    [NotMapped]
    public List<ProjectMember> Students => Members.Where(m => m.Type == MemberType.Student).ToList();
    [NotMapped]
    public List<ProjectMember> Advisors => Members.Where(m => m.Type == MemberType.Advisor).ToList();
    [NotMapped]
    public List<ProjectMember> Liaisons => Members.Where(m => m.Type == MemberType.Liaison).ToList();
}

// In some rare cases (hi francisco!), someone can be both an advisor and a liaison.
[PrimaryKey(nameof(ProjectId), nameof(PersonId), nameof(Type))]
public class ProjectMember
{
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }

    public Project.MemberType Type { get; set; }
}

public class ProjectResource
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(10)]
    public ResourceType Type { get; set; }

    public int? FileId { get; set; }
    public File File { get; set; }

    public string Text { get; set; }

    [MaxLength(2000)]
    public string Url { get; set; }

    public bool IsPrivate { get; set; }
}
