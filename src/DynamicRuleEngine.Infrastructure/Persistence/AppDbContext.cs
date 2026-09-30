using DynamicRuleEngine.Domain.Entities;
using DynamicRuleEngine.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DynamicRuleEngine.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<WorkflowDefinition> Workflows => Set<WorkflowDefinition>();
    public DbSet<ParameterDefinition> Parameters => Set<ParameterDefinition>();
    public DbSet<RuleDefinition> Rules => Set<RuleDefinition>();
    public DbSet<WorkflowJson> WorkflowJsons => Set<WorkflowJson>();
    public DbSet<ScoreDefinition> ScoreDefinitions => Set<ScoreDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowDefinition>(entity =>
        {
            entity.ToTable("WorkflowMaster");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.WorkflowCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.WorkflowName).HasMaxLength(200).IsRequired();
            entity.HasIndex(x => x.WorkflowCode).IsUnique();

            entity.HasMany(x => x.Parameters)
                .WithOne(x => x.Workflow)
                .HasForeignKey(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Rules)
                .WithOne(x => x.Workflow)
                .HasForeignKey(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.WorkflowJson)
                .WithOne(x => x.Workflow)
                .HasForeignKey<WorkflowJson>(x => x.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ParameterDefinition>(entity =>
        {
            entity.ToTable("ParameterDefinition");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ParameterCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DataType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.Weight).HasPrecision(18, 2);
            entity.Property(x => x.WhyItMatters).HasMaxLength(2000);
            entity.Property(x => x.HowToMeasure).HasMaxLength(2000);
            entity.HasIndex(x => new { x.WorkflowId, x.ParameterCode }).IsUnique();
            entity.HasMany(x => x.ScoreDefinitions)
                .WithOne(x => x.ParameterDefinition)
                .HasForeignKey(x => x.ParameterDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<ScoreDefinition>(entity =>
        {
            entity.ToTable("ScoreDefinition");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.HasIndex(x => new { x.ParameterDefinitionId, x.Score }).IsUnique();
        });

        modelBuilder.Entity<RuleDefinition>(entity =>
        {
            entity.ToTable("RuleDefinition");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RuleCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.RuleName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RuleType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(x => x.ConditionExpression).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.CalculationExpression).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ErrorMessage).HasMaxLength(1000);
            entity.Property(x => x.SuccessEvent).HasMaxLength(200);
            entity.HasIndex(x => new { x.WorkflowId, x.RuleCode }).IsUnique();
        });

        modelBuilder.Entity<WorkflowJson>(entity =>
        {
            entity.ToTable("WorkflowJson");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.JsonContent).HasColumnType("nvarchar(max)").IsRequired();
            entity.HasIndex(x => x.WorkflowId).IsUnique();
        });
    }
}
