using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TanaInt.Domain.Calendar;

public class TanaTaskDto
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("url")] public string Url { get; set; }
    [JsonPropertyName("refString")] public string RefString { get; set; }
    [JsonPropertyName("date")] public string Date { get; set; }
    [JsonPropertyName("id")] public string? GCalEventId { get; set; }
    [JsonPropertyName("doneTime")] public string DoneTime { get; set; }
    [JsonPropertyName("scheduled")] public string Scheduled { get; set; }
    [JsonIgnore] public DateTime Start { get; set; }
    [JsonIgnore] public DateTime End { get; set; }
    [JsonIgnore] public bool IsAllDay { get; set; }

    public string FormatName()
    {
        var undoneStatusIndicator =
            Scheduled?.Equals("yes", StringComparison.InvariantCultureIgnoreCase) is true ? "📅" : "⚪";
        return string.IsNullOrWhiteSpace(DoneTime) ? $"{undoneStatusIndicator} {Name}" : $"✅ {Name}";
    }

    public TanaTaskDto ParseInput()
    {
        (IsAllDay, Start, End) = ParseDateToIntervals(Date);
        GCalEventId = string.IsNullOrWhiteSpace(RefString) ? null : ParseGCalRefString(RefString);
        return this;
    }

    private string ParseGCalRefString(string refString) => refString.Split(", ")[1];

    private (bool IsAllDay, DateTime Start, DateTime End) ParseDateToIntervals(string dateString)
    {
        var dateStrings = Utils.ExtractDateStrings(dateString);
        var start = DateTime.Parse(dateStrings[0], CultureInfo.InvariantCulture);
        var isAllDay = (start - start.Date).TotalSeconds == 0 && dateStrings.Length == 1;
        var end = dateStrings.Length > 1
            ? DateTime.Parse(dateStrings[1], CultureInfo.InvariantCulture)
            : (isAllDay ? start.AddDays(1) : start.AddMinutes(30));
        return (isAllDay, start, end);
    }
}