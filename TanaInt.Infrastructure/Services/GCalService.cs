using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using TanaInt.Domain.Calendar;

namespace TanaInt.Infrastructure.Services;

public interface IGCalService
{
    Task<TanaExtRefResponse> SyncToEvent(TanaTaskDto dto);
}

public class GCalService : IGCalService
{
    /* Global instance of the scopes required by this quickstart.
     If modifying these scopes, delete your previously saved token.json/ folder. */
    private readonly string[] _scopes = { CalendarService.Scope.CalendarEvents };
    private const string ApplicationName = "TanaInt Sync Api";
    private const string TokenFileName = "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user";
    private const string WritablePath = "/tmp";
    private const string CredentialsPath = "credentials.json";

    private const string CalendarId =
        "f89c72dc48bd042b626e8abb6f4c7722b58f3d83d377330ec583dc584e32b88b@group.calendar.google.com";

    public async Task<TanaExtRefResponse> SyncToEvent(TanaTaskDto dto)
    {
        UserCredential credential;
        // Load client secrets.
        await using (var stream =
                     new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read))
        {
            /* The file token.json stores the user's access and refresh tokens, and is created
             automatically when the authorization flow completes for the first time. */
            MoveToWritablePath();
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                (await GoogleClientSecrets.FromStreamAsync(stream)).Secrets,
                _scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(WritablePath, true));
            Console.WriteLine("Credential file saved to: " + WritablePath);
        }

        // Create Google Calendar API service.
        var service = new CalendarService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });

        var eventBody = new Event()
        {
            Summary = dto.FormatName(),
            Description = dto.Url,
            Start = dto.IsAllDay
                ? new() { Date = dto.Start.ToString("yyyy-MM-dd") }
                : new() { DateTimeRaw = dto.Start.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00", },
            End = dto.IsAllDay
                ? new() { Date = dto.End.ToString("yyyy-MM-dd") }
                : new() { DateTimeRaw = dto.End.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00", },
            Source = new() { Url = dto.Url },
        };

        if (!string.IsNullOrWhiteSpace(dto.DoneTime))
        {
            eventBody.Reminders.UseDefault = false;
            eventBody.Reminders.Overrides = new List<EventReminder>();
        }
        else
        {
            eventBody.Reminders.UseDefault = true;
            eventBody.Reminders.Overrides = null;
        }

        CalendarBaseServiceRequest<Event> request;
        if (string.IsNullOrWhiteSpace(dto.GCalEventId))
            request = service.Events.Insert(eventBody, CalendarId);
        else
            request = service.Events.Update(eventBody, CalendarId, dto.GCalEventId);

        var result = await request.ExecuteAsync();
        return new TanaExtRefResponse(result.HtmlLink, result.Id);
    }

    private void MoveToWritablePath()
    {
        if (!File.Exists(Path.Combine(WritablePath, TokenFileName)))
            File.Copy(Path.Combine("token", TokenFileName), Path.Combine(WritablePath, TokenFileName));
    }
}