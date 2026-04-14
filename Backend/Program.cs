using Backend.Services;

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

app.MapGet("/api/test", () => "test");

//TODOS
// GET /api/match/{id} – Hämta en match
// PUT /api/match/{id}/player/{playerId}/score – Uppdatera poäng
// PUT /api/match/{id}/player/{playerId}/name – Byt namn på spelare
// POST /api/match/{id}/player – Lägg till spelare
// POST /api/match – Skapa en ny match
// POST /api/match/{id}/reset – Starta om matchen (nollställ poäng)
// POST /api/match/{id}/clone – Skapa en ny match med samma inställningar
// DELETE /api/match/{id}/player/{playerId} – Ta bort spelare

app.MapFallbackToFile("index.html");

app.Run();
