using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    // Should points to the lastest course journal
    public int? CourseJournalId { get; set; }
    [ForeignKey("CourseJournalId")]
    public CourseJournal CourseJournal { get; set; }

    public bool IsObsolete { get; set; }

    public bool IsGraduateCourse => Number.StartsWith("5");
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

    public List<StudentSample> StudentSamples { get; set; }
}

[Table("StudentSamples")]
public class StudentSample
{
    public int Id { get; set; }

    public int CourseJournalId { get; set; }
    public CourseJournal courseJournal { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }
    [Required, MaxLength(255)]
    public string Url { get; set; }
    [MaxLength(255)]
    public string Grade { get; set; }
}
