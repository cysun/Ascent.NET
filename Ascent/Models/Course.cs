using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ascent.Models
{
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

        // ABET Course Description, AKA ABET Syllabus
        public int? AbetDescriptionId { get; set; }
        public File AbetDescription { get; set; }

        public bool IsObsolete { get; set; }

        public bool IsGraduateCourse => Number.StartsWith("5");
    }

    public class Section
    {
        public int Id { get; set; }

        public Term Term { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public int Number { get; set; }

        public int? InstructorId { get; set; }
        public Person Instructor { get; set; }
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

    public class Grade
    {
        [Key, MaxLength(4)]
        public string Symbol { get; set; }

        public decimal? Value { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }
    }
}
