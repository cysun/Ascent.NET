using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Models;

public enum RubricAssessmentType
{
    Instructor = 0,
    Peer = 1,
    External = 2
}

public class RubricDataPoint
{
    public int Id { get; set; }

    public int Year { get; set; }

    public Term Term { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; }

    public RubricAssessmentType AssessmentType { get; set; } = RubricAssessmentType.Instructor;

    public int EvaluatorId { get; set; }
    public Person Evaluator { get; set; }

    public int EvaluateeId { get; set; }
    public Person Evaluatee { get; set; }

    public int RubricId { get; set; }
    public Rubric Rubric { get; set; }

    public int CriterionId { get; set; }
    public RubricCriterion Criterion { get; set; }

    public int RatingId { get; set; }
    public RubricRating Rating { get; set; }
}

// This class is mapped to a view. We aggregate multiple ratings for a person in a section for a rubric criterion
// by taking the rating value average. This allows us to create statistics based on person instead of evaluation
// (one person may receive multiple evaluations of the same rubric, especially in peer evaluation).
[Keyless]
public class RubricDataByPerson
{
    public int Year { get; set; }

    public int TermCode { get; set; } // Keyless entity cannot have owned entities
    public Term Term => new Term(TermCode);

    public int CourseId { get; set; }
    public Course Course { get; set; }

    public RubricAssessmentType AssessmentType { get; set; } = RubricAssessmentType.Instructor;

    public int EvaluateeId { get; set; }
    public Person Evaluatee { get; set; }

    public int RubricId { get; set; }
    public Rubric Rubric { get; set; }

    public int CriterionId { get; set; }
    public RubricCriterion Criterion { get; set; }

    public double AvgRatingValue { get; set; }
}

// This class serves as the return type of the query "select distinct CourseId, Term_Code from RubricData".
// There is no view for this class even though we have to say ToView() to prevent EF Core from creating a table.
[Keyless]
public class AssessmentSection
{
    public int CourseId { get; set; }
    public Course Course { get; set; }

    [Column("Term_Code")]
    public int TermCode { get; set; }

    public Term Term => new Term(TermCode);
}
