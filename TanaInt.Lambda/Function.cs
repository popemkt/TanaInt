using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using TanaInt.Domain;
using TanaInt.Domain.Calendar;
using TanaInt.Infrastructure.Services;

// The function handler that will be called for each Lambda event
var handler = async (APIGatewayHttpApiV2ProxyRequest input, ILambdaContext context) =>
{
    try
    {
        var dto = JsonSerializer.Deserialize<TanaTaskDto>(input.Body, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var result = await new GCalService().SyncToEvent(dto.ParseInput());
        return new APIGatewayHttpApiV2ProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = result.FormatOutput()
        };
    }
    catch (Exception e)
    {
        context.Logger.LogError($"{input.Body}\n{e.Message}\n{e.StackTrace}");
        return new APIGatewayHttpApiV2ProxyResponse() { StatusCode = (int)HttpStatusCode.InternalServerError };
    }
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();