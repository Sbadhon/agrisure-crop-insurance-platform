using AgriSure.BuildingBlocks.Observability;
using AgriSure.Operations.Api.Data;
using AgriSure.Operations.Api.Features;
using AgriSure.Operations.Api.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.AddAgriSureServiceDefaults("agrisure-operations-api");

builder.Services.AddDbContext<OperationsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OperationsDatabase")));
builder.Services.AddHostedService<ClaimEventConsumer>();

var app = builder.Build();
app.UseAgriSureServiceDefaults();
app.MapOperationsEndpoints();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OperationsDbContext>();
    await db.Database.EnsureCreatedAsync();
}

await app.RunAsync();
