using System.ComponentModel.DataAnnotations;

namespace Ascent.Models;

// See https://csns.cysun.org/download?fileId=7838340 for the terminology: rubric, criterion,
// rating, and assessment.

public class Rubric
{
    public int Id { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; }

    public string Description { get; set; }

    public int CriteriaCount { get; set; }

    public List<RubricCriterion> Criteria { get; set; }

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

    // Given a value, find the index of the rating that is closest to the value.
    public int ValueToRatingIndex(double value)
    {
        if (Ratings == null || Ratings.Count == 0) return -1;

        var index = 0;
        for (int i = 1; i < Ratings.Count; ++i)
            if (Math.Abs(Ratings[i].Value - value) <= Math.Abs(Ratings[index].Value - value))
                index = i;
        return index;
    }
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
