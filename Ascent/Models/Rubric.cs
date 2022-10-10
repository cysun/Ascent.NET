using System.ComponentModel.DataAnnotations;

namespace Ascent.Models
{
    // See https://csns.cysun.org/download?fileId=7838340 for the terminology: rubric, criterion,
    // rating, and assessment.

    public class Rubric
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int CriteriaCount { get; set; }

        public DateTime? TimePublished { get; set; }
        public bool IsPublished => TimePublished.HasValue && TimePublished < DateTime.UtcNow;

        public bool IsObsolete { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class RubricCriterion
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; }

        public string Description { get; set; }

        public Rubric Rubric { get; set; }
        public int RubricId { get; set; }

        public int Index { get; set; }

        public List<RubricRating> Ratings { get; set; }
    }

    public class RubricRating
    {
        public int Id { get; set; }

        public RubricCriterion Criterion { get; set; }
        public int CriterionId { get; set; }

        public int Index { get; set; }

        public double Value { get; set; }

        [MaxLength(80)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
