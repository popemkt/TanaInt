using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using TanaInt.Domain.Calendar;

namespace TanaInt.Infrastructure.Services;

public interface ICalendarRecurrenceService
{
    RecurrencePattern ParseRRule(string rrule);
    DateTime NextOccurrence(RecurrencePattern rrule, DateTime? dt = null);
}

public class CalendarRecurrenceService : ICalendarRecurrenceService
{
    private sealed class CustomPatternEvaluator : RecurrencePatternEvaluator
    {
        public CustomPatternEvaluator(RecurrencePattern pattern) : base(pattern)
        {
        }

        public DateTime Increment(DateTime date, RecurrencePattern pattern, int interval)
        {
            var nextDate = date;
            IncrementDate(ref nextDate, pattern, interval);
            return nextDate;
        }
        
        //Taken directly from Ical.Net source, with modified case of FrequencyType.Weekly
        private void IncrementDate(ref DateTime dt, RecurrencePattern pattern, int interval)
        {
            if (interval == 0)
                throw new Exception("Cannot evaluate with an interval of zero.  Please use an interval other than zero.");
            DateTime dt1 = dt;
            switch (pattern.Frequency)
            {
                case FrequencyType.Secondly:
                    dt = dt1.AddSeconds(interval);
                    break;
                case FrequencyType.Minutely:
                    dt = dt1.AddMinutes(interval);
                    break;
                case FrequencyType.Hourly:
                    dt = dt1.AddHours(interval);
                    break;
                case FrequencyType.Daily:
                    dt = dt1.AddDays(interval);
                    break;
                case FrequencyType.Weekly:
                    dt = dt1.AddDays(interval * 7);
                    break;
                case FrequencyType.Monthly:
                    dt = dt1.AddDays(-dt1.Day + 1).AddMonths(interval);
                    break;
                case FrequencyType.Yearly:
                    dt = dt1.AddDays(-dt1.DayOfYear + 1).AddYears(interval);
                    break;
                default:
                    throw new Exception("FrequencyType.NONE cannot be evaluated. Please specify a FrequencyType before evaluating the recurrence.");
            }
        }

    }

    public RecurrencePattern ParseRRule(string rrule)
    {
        return new RecurrencePattern(rrule);
    }

    public DateTime NextOccurrence(RecurrencePattern rrule, DateTime? dt = null)
    {
        var dateTime = dt?.AddDays(1) ?? DateTime.UtcNow.Date.AddDays(1);
        var calDateTime = new CalDateTime(dateTime);
        var evaluator = new CustomPatternEvaluator(rrule);
        var endPeriod = evaluator.Increment(dateTime, rrule, 1);
        var periods = evaluator.Evaluate(calDateTime, dateTime.AddDays(1), endPeriod.AddDays(1), false);
        return periods.First().StartTime.Date;
    }
}