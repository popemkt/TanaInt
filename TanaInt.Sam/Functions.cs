using System.Net;
using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using AWS.Lambda.Powertools.Tracing;
using TanaInt.Domain;
using TanaInt.Infrastructure.Services;

[assembly: LambdaSerializer(typeof(CamelCaseLambdaJsonSerializer))]

namespace TanaInt.Sam;

/// <summary>
/// A collection of sample Lambda functions that provide a REST api for doing simple math calculations. 
/// </summary>
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
        context.Logger.LogInformation(date.ToString());
        return Task.CompletedTask;
    }

    [LambdaFunction(MemorySize = 216, Timeout = 60)]
    [RestApi(LambdaHttpMethod.Post, "/tana-to-gcal")]
    [Tracing]
    public async Task<APIGatewayProxyResponse> PushEventsToGcal([FromBody] TanaTaskDto tanaTaskDto,
        ILambdaContext context)
    {
        context.Logger.LogError(JsonSerializer.Serialize(tanaTaskDto));
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
}