namespace Ascent.Models;

public enum RubricAssessmentType
{
    Instructor,
    Peer,
    External
}

public class RubricDataPoint
{
    public int Id { get; set; }

    public int RubricId { get; set; }
    public Rubric Rubric { get; set; }

    public int CriterionId { get; set; }
    public RubricCriterion Criterion { get; set; }

    public int RatingId { get; set; }
    public RubricRating Rating { get; set; }

    public int EvaluatorId { get; set; }
    public Person Evaluator { get; set; }

    public int EvaluateeId { get; set; }
    public Person Evaluatee { get; set; }

    public RubricAssessmentType AssessmentType { get; set; } = RubricAssessmentType.Instructor;

    public Term Term { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; }
}
