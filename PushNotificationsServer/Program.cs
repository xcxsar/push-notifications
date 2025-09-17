using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using PushNotificationsServer.Services;
using Supabase;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Supabase Configuration
var supabaseUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
var supabaseKey = Environment.GetEnvironmentVariable("SUPABASE_SERVICE_KEY");

builder.Services.AddScoped<Client>( _ =>
{
    var options = new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = true,
    };
    return new Client(supabaseUrl, supabaseKey, options);
});

builder.Services.AddScoped<IDatabaseService, DatabaseService>();

// Firebase Configuration
if (FirebaseApp.DefaultInstance == null)
{
    try 
    {
        var credPath = Path.Combine(AppContext.BaseDirectory, "pushnotifications-13589-firebase-adminsdk-fbsvc-b8fab66b9d.json");

        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(credPath)
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing Firebase: {ex.Message}");
        throw;
    }
}
builder.Build().Run();
