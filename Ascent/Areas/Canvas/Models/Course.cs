using System.Text.Json.Serialization;

namespace Ascent.Areas.Canvas.Models;

public static class EnrollmentType
{
    public const string Teacher = "teacher";
    public const string Student = "student";
    public const string TA = "ta";
    public const string Observer = "observer";
    public const string Designer = "designer";
}

public class Course
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("course_code")]
    public string Code { get; set; }

    [JsonPropertyName("term")]
    public Term Term { get; set; } // Need include[]=term in API call 
}
