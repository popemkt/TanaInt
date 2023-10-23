namespace TanaInt.Domain;

public class TanaExtRefResponse
{
    public static string FormatOutput(string url, string eventId)
    {
        return string.Join('\n',
            $"- {url}",
            $"- {eventId}");
    }
}