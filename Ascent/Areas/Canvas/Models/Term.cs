using System.Text.Json.Serialization;

namespace Ascent.Areas.Canvas.Models;

public class Term
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}
