using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TanaInt.Domain.Calendar;

public class TanaDateTimeDto
{
    [JsonPropertyName("rrule")] public string RRule { get; set; }
    [JsonPropertyName("context")] public string Context { get; set; }
    [JsonIgnore] public DateTime OccurenceDate { get; set; }

    public TanaDateTimeDto ParseInput()
    {
        var lines = Context.Split("\n");
        
        OccurenceDate = ParseDate(lines.First(l => l.StartsWith("  - Date::")));

        return this;
    }

    private DateTime ParseDate(string line)
    {
        var dates = Utils.ExtractDateStrings(line);
        return DateTime.Parse(dates[0], CultureInfo.InvariantCulture);
    }
}