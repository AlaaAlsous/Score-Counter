var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.MapGet("/api/test", () => "test");

app.MapFallbackToFile("index.html");

app.Run();
