using System.Net;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Tracing;
using TanaInt.Domain.Calendar;
using TanaInt.Domain.Srs.Fsrs;
using TanaInt.Domain.WallChanger;
using TanaInt.Infrastructure.Services;

[assembly: LambdaSerializer(typeof(CamelCaseLambdaJsonSerializer))]

namespace TanaInt.Sam;

public class Functions
{
    private readonly IGCalService _gCalService;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// instantiated in <see cref="Startup"/> will be injected here.
    /// 
    /// As an alternative, a dependency could be injected into each 
    /// Lambda function handler via the [FromServices] attribute.
    /// </remarks>
    public Functions(IGCalService gCalService)
    {
        _gCalService = gCalService;
    }

    [LambdaFunction(MemorySize = 216, Timeout = 60)]
    [RestApi(LambdaHttpMethod.Get, "/gcal-to-tana")]
    [Tracing]
    public Task GetEventsFromGcal(ILambdaContext context, [FromQuery] DateTime? date)
    {
        context.Logger.LogInformation(date?.ToString());
        return Task.CompletedTask;
    }


    [LambdaFunction(MemorySize = 216, Timeout = 60)]
    [RestApi(LambdaHttpMethod.Post, "/tana-to-gcal")]
    [Tracing]
    public async Task<APIGatewayProxyResponse> PushEventsToGcal([FromBody] TanaTaskDto tanaTaskDto,
        ILambdaContext context)
    {
        try
        {
            var result = await _gCalService.SyncToEvent(tanaTaskDto.ParseInput());
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = result.FormatOutput()
            };
        }
        catch (Exception e)
        {
            context.Logger.LogError($"{tanaTaskDto}\n{e.Message}\n{e.StackTrace}");
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }

    [LambdaFunction(MemorySize = 216, Timeout = 60)]
    [RestApi(LambdaHttpMethod.Post, "/change-banner")]
    [Tracing]
    public async Task<APIGatewayProxyResponse> ChangeBanner(ILambdaContext context,
        APIGatewayProxyRequest request,
        [FromServices] IBannerChangerService bannerChangerService)
    {
        try
        {
            string bodyText = request.Body;
            var bannerChangerDto = new BannerChangerDto(bodyText);

            var result = await bannerChangerService.ChangeBanner(bannerChangerDto.ParseImages());
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = result
            };
        }
        catch (Exception e)
        {
            context.Logger.LogError($"{request.Body}\n{e.Message}\n{e.StackTrace}");
            throw;
        }
    }

    [LambdaFunction(MemorySize = 216, Timeout = 60)]
    [RestApi(LambdaHttpMethod.Post, "/next-rrule-occurrence")]
    [Tracing]
    public APIGatewayProxyResponse NextRRuleOccurrence(ILambdaContext context,
        [FromBody] TanaDateTimeDto dto,
        [FromServices] ICalendarRecurrenceService calendarRecurrence)
    {
        try
        {
            var parsedDto = dto.ParseInput();
            var result = TanaDateTimeResponse.FormatDate(
                calendarRecurrence.NextOccurrence(calendarRecurrence.ParseRRule(dto.RRule), parsedDto.OccurenceDate));
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = result
            };
        }
        catch (Exception e)
        {
            context.Logger.LogError($"{dto}\n{e.Message}\n{e.StackTrace}");
            throw;
        }
    }

    [LambdaFunction(MemorySize = 216, Timeout = 60)]
    [RestApi(LambdaHttpMethod.Post, "/fsrs")]
    [Tracing]
    public APIGatewayProxyResponse Fsrs(ILambdaContext context,
        [FromBody] FsrsDto dto,
        [FromServices] IFsrsService fsrsService,
        [FromServices] IRequestTimeZoneProvider requestTimeZoneProvider)
    {
        try
        {
            context.Logger.LogInformation($"Dto: {JsonSerializer.Serialize(dto)}");
            requestTimeZoneProvider.ParseAndSetRequestTimeZone(dto.TimeZone);
            var convertedUtcNowToLocalTimezone = requestTimeZoneProvider.Convert(DateTime.UtcNow);
            var parsedDto = dto.ParseInput(convertedUtcNowToLocalTimezone);
            context.Logger.LogInformation($"Parsed dto: {JsonSerializer.Serialize(dto)}");
            context.Logger.LogInformation($"Converted time: {JsonSerializer.Serialize(convertedUtcNowToLocalTimezone)}");
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = fsrsService.Repeat(parsedDto, convertedUtcNowToLocalTimezone)
                    .ToTanaString()
            };
        }
        catch (Exception e)
        {
            context.Logger.LogError($"{dto}\n{e.Message}\n{e.StackTrace}");
            throw;
        }
    }
}