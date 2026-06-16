using AgriSure.BuildingBlocks.Observability;
using AgriSure.Policy.Api.Data;
using AgriSure.Policy.Api.Features;
using AgriSure.Policy.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddAgriSureServiceDefaults("agrisure-policy-api");

builder.Services.AddDbContext<PolicyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PolicyDatabase")));

var app = builder.Build();
app.UseAgriSureServiceDefaults();
app.MapPolicyEndpoints();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PolicyDbContext>();
    await db.Database.EnsureCreatedAsync();
    await PolicySeed.SeedAsync(db);
}

await app.RunAsync();
