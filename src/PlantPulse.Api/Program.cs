using PlantPulse.Api.ErrorHandling;
using PlantPulse.Api.Live;
using PlantPulse.Application.Alarms;
using PlantPulse.Infrastructure;
using PlantPulse.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<AcknowledgeAlarmHandler>();

builder.Services.AddSignalR();
builder.Services.AddHostedService<LiveFeedConsumer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlantPulseDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.MapHub<PlantHub>("/hubs/plant");

app.Run();
