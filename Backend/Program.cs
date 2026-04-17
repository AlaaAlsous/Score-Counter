using Backend.Services;
//using Backend.Models;
using Shared;

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
builder.Services.AddSingleton<MatchStore>();

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors();
app.MapMatchEndpoints();

app.MapFallbackToFile("index.html");

app.Run();
