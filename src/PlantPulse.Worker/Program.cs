using PlantPulse.Application.Readings;
using PlantPulse.Infrastructure;
using PlantPulse.Infrastructure.Outbox;
using PlantPulse.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ReadingProcessor>();

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<OutboxProcessor>();

var host = builder.Build();
host.Run();
