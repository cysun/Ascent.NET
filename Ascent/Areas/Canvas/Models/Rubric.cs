using System.Text.Json.Serialization;

namespace Ascent.Areas.Canvas.Models;

public static class RubricContextType
{
    public const string Course = "course";
}

public static class RubricAssociationType
{
    public const string Course = "Course";
    public const string Assignment = "Assignment";
}

public static class RubricAssessmentType
{
    public const string Grading = "grading";
    public const string PeerReview = "peer_review";
    public const string ProvisionalGrade = "provisional_grade";
}

public class Rubric
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("context_id")]
    public int ContextId { get; set; }

    [JsonPropertyName("context_type")]
    public string ContextType { get; set; } = RubricContextType.Course;

    [JsonPropertyName("free_form_criterion_comments")]
    public bool FreeFormCriterionComments { get; set; }

    [JsonPropertyName("data")]
    public List<RubricCriterion> Criteria { get; set; }

    [JsonPropertyName("associations")]
    public List<RubricAssociation> Associations { get; set; }

    [JsonPropertyName("assessments")]
    public List<RubricAssessment> Assessments { get; set; }
}

// If an assignment has an associated rubric, the Assignment object includes a RubricSettings object.
public class RubricSettings
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
}

public class RubricCriterion
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("long_description")]
    public string LongDescription { get; set; }

    [JsonPropertyName("points")]
    public float Points { get; set; }

    [JsonPropertyName("ratings")]
    public List<RubricRating> Ratings { get; set; }
}

public class RubricRating
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("criterion_id")]
    public string CriterionId { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("long_description")]
    public string LongDescription { get; set; }

    [JsonPropertyName("points")]
    public float Points { get; set; }
}

public class RubricAssociation
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("rubric_id")]
    public int RubricId { get; set; }

    [JsonPropertyName("association_id")]
    public int AssociationId { get; set; }

    [JsonPropertyName("association_type")]
    public string AssociationType { get; set; } = RubricAssociationType.Assignment;

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = "grading";
}

public class RubricAssessment
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("rubric_id")]
    public int RubricId { get; set; }

    [JsonPropertyName("rubric_association_id")]
    public int RubricAssociationId { get; set; }

    [JsonPropertyName("provisional_grade")]
    public string AssessmentType { get; set; }

    [JsonPropertyName("assessor_id")]
    public int AssessorId { get; set; }

    [JsonPropertyName("comments")]
    public string Comments { get; set; }
}
