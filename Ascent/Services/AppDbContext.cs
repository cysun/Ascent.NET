using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Grade> Grades { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<Models.File> Files { get; set; }
    public DbSet<FileHistory> FileHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().HasQueryFilter(n => !n.IsDeleted);
        modelBuilder.Entity<Person>().HasIndex(p => p.CampusId).IsUnique();
        modelBuilder.Entity<Person>().Property(p => p.IsDeleted).HasDefaultValue(false);
        modelBuilder.Entity<Person>().Property(p => p.IsInstructor).HasDefaultValue(false);
        modelBuilder.Entity<Course>().HasAlternateKey(c => new { c.Subject, c.Number });
        modelBuilder.Entity<Enrollment>().HasAlternateKey(e => new { e.SectionId, e.StudentId });
        modelBuilder.Entity<Enrollment>().HasQueryFilter(e => !e.Student.IsDeleted);
        modelBuilder.Entity<Page>().HasQueryFilter(n => !n.IsDeleted);
        modelBuilder.Entity<FileHistory>().HasKey(h => new { h.FileId, h.Version });
    }
}
