using ImportCSNS.Models;
using Microsoft.EntityFrameworkCore;

namespace ImportCSNS;

public class CsnsDbContext : DbContext
{
    private readonly string _connectionString;

    public CsnsDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(_connectionString);

    public DbSet<User> Users { get; set; }
    public DbSet<Models.File> Files { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Rubric> Rubrics { get; set; }
    public DbSet<Rubric> RubricIndicators { get; set; }
    public DbSet<RubricAssignment> RubricAssignments { get; set; }
    public DbSet<RubricSubmission> RubricSubmissions { get; set; }
    public DbSet<RubricEvaluation> RubricEvaluations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectStudent>().HasKey(p => new { p.ProjectId, p.UserId });
        modelBuilder.Entity<ProjectAdvisor>().HasKey(p => new { p.ProjectId, p.UserId });
        modelBuilder.Entity<ProjectLiaison>().HasKey(p => new { p.ProjectId, p.UserId });
        modelBuilder.Entity<ProjectResource>().HasKey(p => new { p.ProjectId, p.ResourceOrder });
        modelBuilder.Entity<RubricIndicatorCriterion>().HasKey(c => new { c.RubricIndicatorId, c.Index });
        modelBuilder.Entity<RubricEvaluationRating>().HasKey(r => new { r.EvaluationId, r.Index });
    }
}
