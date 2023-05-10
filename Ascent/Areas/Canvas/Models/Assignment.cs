using System.Text.Json.Serialization;
using Ascent.Models;

namespace Ascent.Areas.Canvas.Models;

public static class SubmissionType
{
    public const string None = "none";
    public const string OnPaper = "on_paper";
    public const string TA = "ta";
    public const string Observer = "observer";
    public const string Designer = "designer";
}

public class Assignment
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("course_id")]
    public int CourseId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("grade_group_students_individually")]
    public bool GradeGroupStudentsIndividually { get; set; }

    [JsonPropertyName("peer_reviews")]
    public bool PeerReviews { get; set; }

    [JsonPropertyName("group_category_id")]
    public int? GroupCategoryId { get; set; } // for group assignment

    [JsonPropertyName("submission_types")]
    public string[] SubmissionTypes { get; set; } = { "none" };

    [JsonPropertyName("rubric_settings")]
    public RubricSettings RubricSettings { get; set; }
}

public class Submission
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("assignment_id")]
    public int AssignmentId { get; set; }

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("full_rubric_assessment")]
    public SubmissionRubricAssessment RubricAssessment { get; set; }
}

// The rubric assessment object included in a submission in the List Submission API call with
// include[]=full_rubric_assessment parameter.
public class SubmissionRubricAssessment
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("grading")]
    public string AssessmentType { get; set; }

    [JsonPropertyName("assessor_id")]
    public int AssessorId { get; set; }

    [JsonPropertyName("data")]
    public SubmissionRubricRating[] Ratings { get; set; }
}

// The rubric ratings included in a submission in the List Submission API call with
// include[]=full_rubric_assessment parameter.
public class SubmissionRubricRating
{
    [JsonPropertyName("points")]
    public float Points { get; set; }

    [JsonPropertyName("comments")]
    public string Comments { get; set; }

    public bool HasComments => !string.IsNullOrEmpty(Comments);

    public int Value => (int)Points;
}

public class PeerReview
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("asset_type")]
    public string AssetType { get; set; } // Submission

    [JsonPropertyName("asset_id")]
    public int AssetId { get; set; } // Submission Id

    [JsonPropertyName("user_id")]
    public int UserId { get; set; } // Id of the owner of the asset (i.e. the user to be assessed)

    [JsonPropertyName("assessor_id")]
    public int AssessorId { get; set; } // Id of the assessor
}

public class AssignmentForCreation
{
    public class Assignment
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonIgnore]
        public int CourseId { get; set; }
    }

    [JsonPropertyName("assignment")]
    public Assignment AssignmentProperty { get; set; }

    public AssignmentForCreation(AssignmentTemplate template, int courseId)
    {
        AssignmentProperty = new Assignment
        {
            Name = template.Name,
            Description = template.Description,
            CourseId = courseId
        };
    }
}
