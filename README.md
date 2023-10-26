# TanaInt

TanaInt is a project to integrate my personal Tana workflow with Google Calendar.

## Components

The project contains the following key components:

### TanaInt.Lambda

An AWS Lambda function that contains the core integration logic. Handles incoming events, calls GCalService to sync calendar, and returns response.

The Lambda function is deployed as an executable assembly rather than a class library. The `LambdaBootstrapBuilder` starts the Lambda runtime.


### TanaInt.Api

An ASP.NET Core web API and views for local testing and demoing the calendar integration. Calls the GCalService to sync calendar events.

### TanaInt.Domain

Contains the `TanaTaskDto` model representing calendar events. Used by both the API and Lambda function.

### TanaInt.Infrastructure

Implements the `GCalService` for integrating with the Google Calendar API. Called by the API and Lambda function.

### TestCalIntegration

A simple console app for testing the Google Calendar integration works as expected.

## Authentication

The API and Lambda function use a service account for Google Calendar API authentication. The `credentials.json` and `token.json` files contain the credentials.

## Deployment

Currently we are only deploying zip manually

### Deploying with .NET Lambda Tools
The [Amazon.Lambda.Tools](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) .NET Global Tool can be used to deploy the function from the command line.

Install the Amazon.Lambda.Tools:

```shell
dotnet tool install -g Amazon.Lambda.Tools
```
Deploy the function:
```shell
cd TanaInt.Lambda
dotnet lambda deploy-function
```

This will package and deploy the function to AWS Lambda using the configuration in aws-lambda-tools-defaults.json (currently it will fail at deploy if unauthenticated, only the zip is created, which is enough).

The configuration includes the target Lambda runtime, handler mapping, memory size, timeout, and IAM role.

To update the configuration, edit aws-lambda-tools-defaults.json.

To invoke the function:

```shell
dotnet lambda invoke-function <function-name>
```
Logs and other commands are available through `dotnet lambda help`.

## Configuration

The `tana-template.json` file is for the template of the request from Tana.

## External Resourecs
- Lambda is hosted with root IAM user: `popemkt1@gmail.com`.
- Function url, [credentials.json](TanaInt.Api/credentials.json), [My google user token](TanaInt.Api/token/Google.Apis.Auth.OAuth2.Responses.TokenResponse-user) are stored in [Prd Env](https://github.com/popemkt/TanaInt/settings/environments/1470884971/edit) secrets.
