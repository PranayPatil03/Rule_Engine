using DynamicRuleEngine.Application.Interfaces;
using DynamicRuleEngine.Application.Services;
using DynamicRuleEngine.Infrastructure.Persistence;
using DynamicRuleEngine.Infrastructure.Framework;
using DynamicRuleEngine.Infrastructure.Repositories;
using DynamicRuleEngine.Infrastructure.Rules;
using DynamicRuleEngine.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicRuleEngine.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDynamicRuleEngineInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<IParameterRepository, ParameterRepository>();
        services.AddScoped<IRuleRepository, RuleRepository>();
        services.AddScoped<IWorkflowJsonRepository, WorkflowJsonRepository>();

        services.AddScoped<IParameterService, ParameterService>();
        services.AddScoped<IRuleService, RuleService>();
        services.AddScoped<IWorkflowService, WorkflowService>();
        services.AddScoped<IRuleEngineService, RuleEngineService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IFrameworkService, FrameworkService>();

        return services;
    }
}
