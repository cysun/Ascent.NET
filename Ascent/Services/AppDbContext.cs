using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Page> Pages { get; set; }
    public DbSet<Models.File> Files { get; set; }
    public DbSet<FileHistory> FileHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Page>().HasQueryFilter(n => !n.Deleted);
        modelBuilder.Entity<FileHistory>().HasKey(h => new { h.FileId, h.Version });
    }
}
