using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

[Table("rubric_assignments")]
public class RubricAssignment
{
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("rubric_id")]
    public long RubricId { get; set; }
    public Rubric Rubric { get; set; }

    [Column("section_id")]
    public long SectionId { get; set; }
    public Section Section { get; set; }

    [Column("deleted")]
    public bool IsDeleted { get; set; }
}

[Table("rubric_submissions")]
public class RubricSubmission
{
    [Column("id")]
    public long Id { get; set; }

    [Column("student_id")]
    public long StudentId { get; set; }
    public User Student { get; set; }

    [Column("assignment_id")]
    public long AssignmentId { get; set; }
    public RubricAssignment Assignment { get; set; }
}

[Table("rubric_evaluations")]
public class RubricEvaluation
{
    [Column("id")]
    public long Id { get; set; }

    [Column("type")]
    public string Type { get; set; }

    [Column("submission_id")]
    public long SubmissionId { get; set; }
    public RubricSubmission Submission { get; set; }

    [Column("evaluator_id")]
    public long EvaluatorId { get; set; }
    public User Evaluator { get; set; }

    public List<RubricEvaluationRating> ratings { get; set; } = new List<RubricEvaluationRating>();

    [Column("comments")]
    public string Comments { get; set; }

    [Column("completed")]
    public bool IsCompleted { get; set; }

    [Column("deleted")]
    public bool IsDeleted { get; set; }
}

[Table("rubric_evaluation_ratings")]
public class RubricEvaluationRating
{
    [Column("evaluation_id")]
    public long EvaluationId { get; set; }
    public RubricEvaluation Evaluation { get; set; }

    [Column("rating_order")]
    public int Index { get; set; }

    [Column("rating")]
    public int Rating { get; set; }
}
