using System.Text.Json;
using Ascent.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
    public DbSet<PageRevision> PageRevisions { get; set; }
    public DbSet<Models.File> Files { get; set; }
    public DbSet<FileRevision> FileRevisions { get; set; }
    public DbSet<MftScore> MftScores { get; set; }
    public DbSet<MftIndicator> MftIndicators { get; set; }
    public DbSet<MftDistributionType> MftDistributionTypes { get; set; }
    public DbSet<MftDistribution> MftDistributions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().HasQueryFilter(n => !n.IsDeleted);
        modelBuilder.Entity<Person>().HasIndex(p => p.CampusId).IsUnique();
        modelBuilder.Entity<Person>().Property(p => p.IsDeleted).HasDefaultValue(false);
        modelBuilder.Entity<Course>().HasAlternateKey(c => new { c.Subject, c.Number });
        modelBuilder.Entity<Enrollment>().HasAlternateKey(e => new { e.SectionId, e.StudentId });
        modelBuilder.Entity<Enrollment>().HasQueryFilter(e => !e.Student.IsDeleted);
        modelBuilder.Entity<Page>().HasQueryFilter(n => !n.IsDeleted);
        modelBuilder.Entity<PageRevision>().HasKey(r => new { r.PageId, r.Version });
        modelBuilder.Entity<PageRevision>().HasQueryFilter(r => !r.Page.IsDeleted);
        modelBuilder.Entity<FileRevision>().HasKey(r => new { r.FileId, r.Version });
        modelBuilder.Entity<MftIndicator>().Property(i => i.Percentiles).HasDefaultValueSql("'{null, null, null}'");
        modelBuilder.Entity<MftDistribution>().HasAlternateKey(d => new { d.Year, d.TypeKey });

        // We'll create/replace Ranks as a whole instead of adding/removing individual entries, so the
        // ValueComparer is mainly for show (and to shut up the EF Core warning). See
        // https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions and
        // https://stackoverflow.com/questions/53050419/json-serialization-value-conversion-not-tracking-changes-with-ef-core
        // for more serious value comparers.
        modelBuilder.Entity<MftDistribution>().Property(d => d.Ranks)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)default),
                v => JsonSerializer.Deserialize<SortedDictionary<int, int>>(v, (JsonSerializerOptions)default),
                new ValueComparer<SortedDictionary<int, int>>((d1, d2) => d1 == d2, d => d.GetHashCode(), d => d)
            );
    }
}
