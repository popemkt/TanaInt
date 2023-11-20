using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TanaInt.Domain.Calendar;

public class TanaTaskDto
{
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("url")] public string Url { get; set; }
    [JsonPropertyName("context")] public string Context { get; set; }
    [JsonPropertyName("refString")] public string RefString { get; set; }
    [JsonPropertyName("id")] public string? GCalEventId { get; set; }
    [JsonIgnore] public DateTime Start { get; set; }
    [JsonIgnore] public DateTime End { get; set; }
    [JsonIgnore] public bool IsAllDay { get; set; }
    [JsonPropertyName("doneTime")] public string DoneTime { get; set; }

    public string FormatName()
    {
        return string.IsNullOrWhiteSpace(DoneTime) ? $"⚪ {Name}" : $"✅ {Name}";
    }

    public TanaTaskDto ParseInput()
    {
        var lines = Context.Split("\n");
        (IsAllDay, Start, End) = ParseDates(lines.Last(l => l.StartsWith("  - Date::")));
        GCalEventId = string.IsNullOrWhiteSpace(RefString) ? null : ParseRefString(RefString);
        DoneTime = ParseDoneTime(lines.FirstOrDefault()?.Substring(0, 5));
        return this;
    }

    private string ParseDoneTime(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return string.Empty;

        if (status.Contains("x", StringComparison.InvariantCultureIgnoreCase))
            return status;

        return string.Empty;
    }

    private string ParseRefString(string refString) => refString.Split(", ")[1];

    private (bool IsAllDay, DateTime Start, DateTime End) ParseDates(string line)
    {
        const string pattern = @"\[\[(.*?)\]\]";

        // Create a Regex object and match the pattern
        var match = Regex.Match(line, pattern, RegexOptions.Compiled);

        var dates = match.Groups[1].Value.Substring(match.Groups[1].Value.IndexOf(":") + 1).Split("/");
        var start = DateTime.Parse(dates[0], CultureInfo.InvariantCulture);
        var isAllDay = (start - start.Date).TotalSeconds == 0 && dates.Length == 1;
        var end = dates.Length > 1
            ? DateTime.Parse(dates[1], CultureInfo.InvariantCulture)
            : (isAllDay ? start.AddDays(1) : start.AddMinutes(30));
        return (isAllDay, start, end);
    }
}