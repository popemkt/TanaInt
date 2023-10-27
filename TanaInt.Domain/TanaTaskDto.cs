﻿using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TanaInt.Domain;

public class TanaTaskDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("context")]
    public string Context { get; set; }
    [JsonPropertyName("refString")]
    public string RefString { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonIgnore]
    public DateTime Start { get; set; }
    [JsonIgnore]
    public DateTime End { get; set; }
    [JsonIgnore]
    public bool IsAllDay { get; set; }
    [JsonPropertyName("doneTime")]
    public string DoneTime { get; set; }

    public string FormatName()
    {
        return string.IsNullOrWhiteSpace(DoneTime) ? $"⚪ {Name}" : $"✅ {Name}";
    }
    
    public TanaTaskDto ParseInput()
    {
        var lines = Context.Split("\n");
        (IsAllDay, Start, End) = ParseDates(lines.Last(l => l.StartsWith("  - Date::")));
        Id = string.IsNullOrWhiteSpace(RefString) ? null : ParseRefString(RefString);
        return this;
    }

    private string ParseRefString(string refString) => refString.Split(", ")[1];

    private (bool IsAllDay, DateTime Start, DateTime End) ParseDates(string line)
    {
        string pattern = @"\[\[(.*?)\]\]";

        // Create a Regex object and match the pattern
        Regex regex = new Regex(pattern);
        var match = regex.Match(line);

        var dates = match.Groups[1].Value.Substring(match.Groups[1].Value.IndexOf(":") + 1).Split("/");
        var start = DateTime.Parse(dates[0], CultureInfo.InvariantCulture);
        var isAllDay = (start - start.Date).TotalSeconds == 0 && dates.Length == 1;
        var end = dates.Length > 1
            ? DateTime.Parse(dates[1], CultureInfo.InvariantCulture)
            : (isAllDay ? start.AddDays(1) : start.AddMinutes(30));
        return (isAllDay, start, end);
    }
}