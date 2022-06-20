using System.ComponentModel.DataAnnotations;

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
}
