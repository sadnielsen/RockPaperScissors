using RockPaperScissors.Core;
using RockPaperScissors.Web;
using RockPaperScissors.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Composition root. Rules are stateless and shared; Random is not thread-safe, so each circuit
// (scope) gets its own, together with its own session factory.
builder.Services.AddSingleton<Rules>();
builder.Services.AddScoped(_ => new Random());
builder.Services.AddScoped<GameSessionFactory>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
