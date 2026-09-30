using DynamicRuleEngine.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddDynamicRuleEngineInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.MapControllers();

app.Run();
