using AgriSure.BuildingBlocks.Messaging;
using AgriSure.BuildingBlocks.Observability;
using AgriSure.Claims.Api.Data;
using AgriSure.Claims.Api.Features;
using AgriSure.Claims.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddAgriSureServiceDefaults("agrisure-claims-api");

builder.Services.AddDbContext<ClaimsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ClaimsDatabase")));
builder.Services.AddHttpClient<PolicyClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PolicyApi"]
        ?? "http://localhost:5101");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<OutboxPublisherService>();

var app = builder.Build();
app.UseAgriSureServiceDefaults();
app.MapClaimEndpoints();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
    await db.Database.EnsureCreatedAsync();
    //await ClaimsSeed.SeedAsync(db);
}

await app.RunAsync();
