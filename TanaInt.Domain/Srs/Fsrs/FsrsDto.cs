using System.Text.Json.Serialization;

namespace TanaInt.Domain.Srs.Fsrs;

public class FsrsDto
{
    [JsonPropertyName("fsrsString")] public string? FsrsString { get; set; }
    [JsonPropertyName("fsrsParameters")] public double[]? FsrsParameters { get; set; }
    [JsonPropertyName("timezone")] public string TimeZone { get; set; }
    [JsonPropertyName("requestRetention")] public double? RequestRetention { get; set; }
    [JsonPropertyName("maxInterval")] public int? MaxIntervalInDays { get; set; }

    [JsonPropertyName("rating")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Rating Rating { get; set; }

    [JsonIgnore] public Card? Card { get; set; }

    public FsrsDto ParseInput()
    {
        Card = Card.FromTanaString(FsrsString);
        return this;
    }
}