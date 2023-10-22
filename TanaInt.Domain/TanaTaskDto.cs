using System.Globalization;
using System.Text.RegularExpressions;

namespace TanaInt.Domain;

public class TanaTaskDto
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Context { get; set; }
    public string RefString { get; set; }
    public string Id { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    public TanaTaskDto ParseInput()
    {
        var lines = Context.Split("\n");
        (Start, End) = ParseDates(lines.Last(l => l.Contains("- Date::")));
        Id = string.IsNullOrWhiteSpace(RefString) ? null : ParseRefString(RefString);
        return this;
    }


    private string ParseRefString(string refString) => refString.Split(", ")[1];

    private (DateTime Start, DateTime End) ParseDates(string line)
    {
        string pattern = @"\[\[(.*?)\]\]";

        // Create a Regex object and match the pattern
        Regex regex = new Regex(pattern);
        var match = regex.Match(line);

        var dates = match.Groups[1].Value.Substring(match.Groups[1].Value.IndexOf(":") + 1).Split("/");
        var start = DateTime.Parse(dates[0], CultureInfo.InvariantCulture);
        var isAllDay = (start - start.Date).Seconds == 0 && dates.Length == 1;
        var end = dates.Length > 1
            ? DateTime.Parse(dates[1], CultureInfo.InvariantCulture)
            : (isAllDay ? start.AddDays(1) : start.AddMinutes(30));
        return (start, end);
    }
}