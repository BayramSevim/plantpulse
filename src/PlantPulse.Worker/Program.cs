using PlantPulse.Application.Readings;
using PlantPulse.Infrastructure;
using PlantPulse.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ReadingProcessor>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
