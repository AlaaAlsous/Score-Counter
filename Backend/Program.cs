using Backend.Services;
//using Backend.Models;
using Shared;
using Backend.Database;
using Microsoft.EntityFrameworkCore;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddScoped<MatchStore>();
builder.Services.AddSignalR();

if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("Using local db");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=scorecounter.db"));
}
else
{
    var keyVaultUri = "https://KV-ScoreCounter.vault.azure.net";
    var client = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
    var secret = await client.GetSecretAsync("SqlScoreCounterConnectionString");
    var connectionString = secret.Value.Value;
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));
}

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var retries = 0;
while (retries < 5)
{
    try
    {
        db.Database.EnsureCreated();
        break;
    }
    catch (Exception ex)
    {
        retries++;
        Console.WriteLine($"DB not ready, retry {retries}/5: {ex.Message}");
        await Task.Delay(TimeSpan.FromSeconds(5));
    }
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors();
app.MapMatchEndpoints();
app.MapHub<MatchEventHub>("/matchevents");
app.MapFallbackToFile("index.html");

app.Run();
