using Backend.Services;
//using Backend.Models;
using Shared;
using Backend.Database;
using Microsoft.EntityFrameworkCore;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;

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
        options.UseSqlServer(connectionString));
}

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.EnsureCreated();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors();
app.MapMatchEndpoints();

app.MapFallbackToFile("index.html");

app.Run();
