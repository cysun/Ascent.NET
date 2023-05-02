using System.Text.Json.Serialization;

namespace Ascent.Areas.Canvas.Models;

public class Group
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("members_count")]
    public int MemberCount { get; set; }

    [JsonPropertyName("group_category_id")]
    public int GroupCategoryId { get; set; }
}

public class GroupMembership
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("group_id")]
    public int GroupId { get; set; }

    [JsonPropertyName("user_id")]
    public int UserId { get; set; }
}
