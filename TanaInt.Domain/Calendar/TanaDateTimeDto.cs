using System.Globalization;
using System.Text.Json.Serialization;

namespace TanaInt.Domain.Calendar;

public class TanaDateTimeDto
{
    [JsonPropertyName("rrule")] public string RRule { get; set; }
    [JsonPropertyName("date")] public string Date { get; set; }
    [JsonIgnore] public DateTime OccurenceDate { get; set; }

    public TanaDateTimeDto ParseInput()
    {
        OccurenceDate = ParseDate(Date);
        return this;
    }

    private DateTime ParseDate(string dateString)
    {
        var dates = Utils.ExtractDateStrings(dateString);
        return DateTime.Parse(dates[0], CultureInfo.InvariantCulture);
    }
}