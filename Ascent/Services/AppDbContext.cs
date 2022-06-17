using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<Models.File> Files { get; set; }
    public DbSet<FileHistory> FileHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().HasQueryFilter(n => !n.IsDeleted);
        modelBuilder.Entity<Person>().HasIndex(p => p.CampusId).IsUnique();
        modelBuilder.Entity<Person>().Property(p => p.IsDeleted).HasDefaultValue(false);
        modelBuilder.Entity<Person>().Property(p => p.IsInstructor).HasDefaultValue(false);
        modelBuilder.Entity<Page>().HasQueryFilter(n => !n.IsDeleted);
        modelBuilder.Entity<FileHistory>().HasKey(h => new { h.FileId, h.Version });
    }
}
