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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectStudent>().HasKey(p => new { p.ProjectId, p.UserId });
        modelBuilder.Entity<ProjectAdvisor>().HasKey(p => new { p.ProjectId, p.UserId });
        modelBuilder.Entity<ProjectLiaison>().HasKey(p => new { p.ProjectId, p.UserId });
        modelBuilder.Entity<ProjectResource>().HasKey(p => new { p.ProjectId, p.ResourceOrder });
    }
}
