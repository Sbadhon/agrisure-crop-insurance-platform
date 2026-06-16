using AgriSure.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args);
builder.AddAgriSureServiceDefaults("agrisure-gateway");
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.UseAgriSureServiceDefaults();
app.MapReverseProxy();
await app.RunAsync();
