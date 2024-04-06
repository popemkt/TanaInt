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