using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Models;

public class Course
{
    public int Id { get; set; }

    [Required, MaxLength(6)]
    public string Subject { get; set; } // e.g. CS

    [Required, MaxLength(6)]
    public string Number { get; set; } // e.g. 3220

    public string Code => $"{Subject} {Number}"; // e.g. CS 3220

    [Required, MaxLength(255)]
    public string Title { get; set; }

    public int MinUnits { get; set; } = 3;
    public int MaxUnits { get; set; } = 3;

    public string Units => MinUnits == MaxUnits ? MinUnits.ToString() : $"{MinUnits}-{MaxUnits}";

    public string CatalogDescription { get; set; }

    public List<CourseCoordinator> Coordinators { get; set; } = new List<CourseCoordinator>();

    // Should points to the lastest course journal
    public int? CourseJournalId { get; set; }
    [ForeignKey("CourseJournalId")]
    public CourseJournal CourseJournal { get; set; }

    public bool IsObsolete { get; set; }

    public bool IsGraduateCourse => Number.StartsWith("5");
}

[PrimaryKey(nameof(CourseId), nameof(PersonId))]
public class CourseCoordinator
{
    public int CourseId { get; set; }
    public Course Course { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }
}

public class CourseJournal
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    [ForeignKey("CourseId")]
    public Course Course { get; set; }

    public Term Term { get; set; }

    public int InstructorId { get; set; }
    public Person Instructor { get; set; }

    [Required, MaxLength(255)]
    public string CourseUrl { get; set; }

    [Required, MaxLength(255)]
    public string SyllabusUrl { get; set; }

    public List<SampleStudent> SampleStudents { get; set; } = new List<SampleStudent>();
}

public class SampleStudent
{
    public int Id { get; set; }

    public int CourseJournalId { get; set; }
    public CourseJournal courseJournal { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string Grade { get; set; }

    [Required, MaxLength(255)]
    public string Url { get; set; }
}
