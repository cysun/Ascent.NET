using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ascent.Models;

public class Section
{
    public int Id { get; set; }

    public Term Term { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; }

    public int InstructorId { get; set; }
    public Person Instructor { get; set; }
}

public class Grade
{
    [Key, MaxLength(4)]
    public string Symbol { get; set; }

    public decimal? Value { get; set; }

    [MaxLength(255)]
    public string Description { get; set; }
}

public class Enrollment
{
    public int Id { get; set; }

    public int SectionId { get; set; }
    public Section Section { get; set; }

    public int StudentId { get; set; }
    public Person Student { get; set; }

    public string GradeSymbol { get; set; }
    [ForeignKey("GradeSymbol")]
    public Grade Grade { get; set; }
}
