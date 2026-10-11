using RockPaperScissors.Core;
using RockPaperScissors.Core.Predictors;
using RockPaperScissors.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Composition root. Scoped means one computer player, with its own Random, per circuit (browser tab):
// Random is not thread-safe, so it must not be shared between users.
builder.Services.AddScoped<IComputerPlayer>(_ => new ComputerPlayer(
    [new FrequencyPredictor(), new LastMovePredictor(), new LastMoveAndOutcomePredictor(), new LastTwoMovesPredictor()],
    new Random(),
    ComputerPlayer.DefaultExplorationRate));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
