using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using TanaInt.Domain;

namespace TanaInt.Api.Services;

public interface IGCalService
{
    Task<string> SyncEvent(TanaTaskDto dto);
}

public class GCalService : IGCalService
{
    /* Global instance of the scopes required by this quickstart.
     If modifying these scopes, delete your previously saved token.json/ folder. */
    static string[] Scopes = { CalendarService.Scope.CalendarEvents };
    static string ApplicationName = "TanaInt Sync Api";
    private static readonly string TokenFileName = "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user";
    private static readonly string WritablePath = "/tmp";
    private static readonly string CredentialsPath = "credentials.json";

    public async Task<string> SyncEvent(TanaTaskDto dto)
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
                Scopes,
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
            Summary = dto.Name,
            Description = dto.Url,
            Start = new()
            {
                DateTimeRaw = dto.Start.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00",
            },
            End = new()
            {
                DateTimeRaw = dto.End.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00",
            },
            Source = new Event.SourceData()
            {
                Url = dto.Url
            },
        };

        string calId = "f89c72dc48bd042b626e8abb6f4c7722b58f3d83d377330ec583dc584e32b88b@group.calendar.google.com";
        CalendarBaseServiceRequest<Event> request;
        if (string.IsNullOrWhiteSpace(dto.Id))
            request = service.Events.Insert(eventBody, calId);
        else
        {
            var currentEvent = await service.Events.Get(calId, dto.Id).ExecuteAsync();

            if (DateTimeOffset.Parse(currentEvent.Start.DateTimeRaw).Date == dto.Start.Date)
                request = service.Events.Update(eventBody, calId, dto.Id);
            else
                request = service.Events.Insert(eventBody, calId);
        }

        var result = await request.ExecuteAsync();
        return $"{result.HtmlLink}\n{result.Id}";
    }

    private void MoveToWritablePath()
    {
        if (!File.Exists(Path.Combine(WritablePath, TokenFileName)))
            File.Copy(Path.Combine("token", TokenFileName), Path.Combine(WritablePath, TokenFileName));
    }
}