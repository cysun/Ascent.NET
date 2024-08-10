using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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

    public int? AbetSyllabusId { get; set; }
    public File AbetSyllabus { get; set; }

    public List<CourseCoordinator> Coordinators { get; set; } = new List<CourseCoordinator>();

    // Course is the "principal" side (no foreign key) of the one-to-one relationship.
    // See https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-one
    public CourseJournal CourseJournal { get; set; }

    // Certain courses like Directed Study and Cooperative Education are considered neither required nor elective.
    public bool IsRequired { get; set; }
    public bool IsElective { get; set; }

    // Whether the course is a "service" course, i.e. not in CS BS/MS curriculum
    public bool IsService { get; set; }

    public bool IsObsolete { get; set; }

    public bool IsGraduateCourse => Number.StartsWith("5");

    public bool IsNotCS => Subject != "CS";
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

    [MaxLength(255)]
    public string SampleStudentWorkUrl { get; set; }
}
