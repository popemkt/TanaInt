namespace TanaInt.Domain;

public class TanaExtRefResponse
{
    public TanaExtRefResponse(string url, string eventId)
    {
        Url = url;
        EventId = eventId;
    }

    public string Url { get; }
    public string EventId { get; }

    public string FormatOutput()
    {
        return string.Join('\n',
            $"- {Url}",
            $"- {EventId}");
    }
}