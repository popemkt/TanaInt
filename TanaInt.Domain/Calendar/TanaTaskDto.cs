﻿using System.Globalization;
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
    [JsonPropertyName("scheduled")] public string Scheduled { get; set; }

    public string FormatName()
    {
        var undoneStatusIndicator =
            Scheduled?.Equals("yes", StringComparison.InvariantCultureIgnoreCase) is true ? "📅" : "⚪";
        return string.IsNullOrWhiteSpace(DoneTime) ? $"{undoneStatusIndicator} {Name}" : $"✅ {Name}";
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
        var dateStrings = Utils.ExtractDateStrings(line);
        var start = DateTime.Parse(dateStrings[0], CultureInfo.InvariantCulture);
        var isAllDay = (start - start.Date).TotalSeconds == 0 && dateStrings.Length == 1;
        var end = dateStrings.Length > 1
            ? DateTime.Parse(dateStrings[1], CultureInfo.InvariantCulture)
            : (isAllDay ? start.AddDays(1) : start.AddMinutes(30));
        return (isAllDay, start, end);
    }
}