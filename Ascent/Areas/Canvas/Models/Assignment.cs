using System.Text.Json.Serialization;

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
