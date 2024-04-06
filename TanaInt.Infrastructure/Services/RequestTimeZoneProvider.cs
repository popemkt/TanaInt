namespace TanaInt.Infrastructure.Services;

public class RequestTimeZoneProvider : IRequestTimeZoneProvider
{
    public TimeZoneInfo TimeZone { get; private set; } = TimeZoneInfo.FindSystemTimeZoneById("Asia/Saigon");
    public TimeZoneInfo ParseAndSetRequestTimeZone(string timeZoneId)
    {
       TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
       return TimeZone;
    }

    public DateTime Convert(DateTime dateTime) => TimeZoneInfo.ConvertTime(dateTime, TimeZone);
}

public interface IRequestTimeZoneProvider
{
    TimeZoneInfo TimeZone { get; }
    TimeZoneInfo ParseAndSetRequestTimeZone(string timeZoneId);
    DateTime Convert(DateTime dateTime);
}
