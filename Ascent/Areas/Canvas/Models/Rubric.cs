using System.Text.Json.Serialization;
using Ascent.Helpers;

namespace Ascent.Areas.Canvas.Models;

public static class RubricContextType
{
    public const string Course = "Course";
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

    [JsonPropertyName("data")]
    public List<RubricCriterion> Criteria { get; set; }
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

// I think Canvas's REST API is not very well designed, and the documentation is incomplete and
// confusing at places. In the case of rubrics, the rubric object retrieved from Canvas and the
// object used to create a rubric on Canvas have different formats. On top of that, to create
// a rubric, criteria and ratings have to be objects (called "hash" in Canvas API docummentation)
// with properties like "1", "2", "3" instead of arrays. The example that worked is at
// https://community.canvaslms.com/t5/Canvas-Question-Forum/Creating-a-rubric-using-REST-API/td-p/478191
public class RubricForCreation
{
    public class Rating
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("long_description")]
        public string LongDescription { get; set; }

        [JsonPropertyName("points")]
        public double Points { get; set; }
    }

    public class Criterion
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("long_description")]
        public string LongDescription { get; set; }

        [JsonPropertyName("ratings")]
        public Dictionary<string, Rating> Ratings { get; set; } = new Dictionary<string, Rating>();
    }

    public class Rubric
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("criteria")]
        public Dictionary<string, Criterion> Criteria { get; set; } = new Dictionary<string, Criterion>();
    }

    public class RubricAssociation
    {
        [JsonPropertyName("association_type")]
        public string AssociationType { get; set; } = RubricAssociationType.Course;

        [JsonPropertyName("association_id")]
        public int AssociationId { get; set; }
    }

    [JsonPropertyName("rubric")]
    public Rubric RubricProperty { get; set; }

    [JsonPropertyName("rubric_association")]
    public RubricAssociation Association { get; set; }

    public RubricForCreation(Ascent.Models.Rubric rubric, int courseId)
    {
        Association = new RubricAssociation { AssociationId = courseId };
        RubricProperty = new Rubric { Title = rubric.Name };
        for (int i = 0; i < rubric.Criteria.Count; i++)
        {
            var criterion = new Criterion
            {
                Description = rubric.Criteria[i].Name,
                LongDescription = Utils.StripHtmlTags(rubric.Criteria[i].Description)
            };
            for (int j = 0; j < rubric.Criteria[i].Ratings.Count; j++)
            {
                criterion.Ratings.Add((j + 1).ToString(), new Rating
                {
                    Description = rubric.Criteria[i].Ratings[j].Name,
                    LongDescription = Utils.StripHtmlTags(rubric.Criteria[i].Ratings[j].Description),
                    Points = rubric.Criteria[i].Ratings[j].Value
                });
            }
            RubricProperty.Criteria.Add((i + 1).ToString(), criterion);
        }
    }
}
