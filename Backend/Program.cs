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

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors();

app.MapGet("/api/test", () => "test");

app.MapFallbackToFile("index.html");

app.Run();
