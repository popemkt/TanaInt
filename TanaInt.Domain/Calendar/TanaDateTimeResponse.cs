namespace TanaInt.Domain.Calendar;

public static class TanaDateTimeResponse
{
    public static string FormatDate(DateTime date)
    {
        return Utils.FormatDate(date);
    }
}