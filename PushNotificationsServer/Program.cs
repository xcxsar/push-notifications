using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

if (FirebaseApp.DefaultInstance == null)
{
    FirebaseApp.Create(new FirebaseAdmin.AppOptions()
    {
        Credential = GoogleCredential.FromFile("pushnotifications-13589-firebase-adminsdk-fbsvc-b8fab66b9d.json")
    });
}

builder.Build().Run();
