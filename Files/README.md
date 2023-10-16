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

The API can be run locally and deployed as a Docker container.

The Lambda function can be deployed using Visual Studio or the .NET Lambda CLI tools.

## Configuration

The `tana-template.json` file is for the template of the request from Tana.

## External Resourecs
- Lambda is hosted with root IAM user: `popemkt1@gmail.com`.
- Function url is stored in [Prd Env](https://github.com/popemkt/TanaInt/settings/environments/1470884971/edit) secret. 