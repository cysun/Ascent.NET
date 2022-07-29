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
    public DbSet<MftScoreStat> MftScoreStats { get; set; }
    public DbSet<MftIndicator> MftIndicators { get; set; }
    public DbSet<MftDistributionType> MftDistributionTypes { get; set; }
    public DbSet<MftDistribution> MftDistributions { get; set; }
    public DbSet<Survey> Surveys { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public DbSet<SurveyAnswer> SurveyAnswers { get; set; }

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
        modelBuilder.Entity<MftScore>().HasAlternateKey(s => new { s.Year, s.StudentId });
        modelBuilder.Entity<MftIndicator>().Property(i => i.Percentiles).HasDefaultValueSql("'{null, null, null}'");
        modelBuilder.Entity<MftDistributionType>().HasAlternateKey(t => t.Alias);
        modelBuilder.Entity<MftDistribution>().HasAlternateKey(d => new { d.Year, d.TypeAlias });
        modelBuilder.Entity<MftDistribution>().HasOne(d => d.Type).WithMany()
            .HasForeignKey(d => d.TypeAlias).HasPrincipalKey(t => t.Alias);
        modelBuilder.Entity<Survey>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<SurveyQuestion>().HasQueryFilter(q => !q.Survey.IsDeleted);
        modelBuilder.Entity<SurveyQuestion>().HasAlternateKey(q => new { q.SurveyId, q.Index });
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.Type).HasConversion<string>();
        modelBuilder.Entity<SurveyResponse>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<SurveyResponse>().HasQueryFilter(r => !r.Survey.IsDeleted);
        modelBuilder.Entity<SurveyAnswer>().HasQueryFilter(a => !a.Response.IsDeleted);
        modelBuilder.Entity<SurveyAnswer>().HasAlternateKey(a => new { a.ResponseId, a.QuestionId });

        // We'll create/replace Ranks as a whole instead of adding/removing individual entries, so the
        // ValueComparer is mainly for show (and to shut up the EF Core warning). See
        // https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions and
        // https://stackoverflow.com/questions/53050419/json-serialization-value-conversion-not-tracking-changes-with-ef-core
        // for more serious value comparers.
        modelBuilder.Entity<MftDistribution>().Property(d => d.Ranks)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions() { IncludeFields = true }),
                v => JsonSerializer.Deserialize<List<(int, int)>>(v, new JsonSerializerOptions() { IncludeFields = true }),
                new ValueComparer<List<(int, int)>>((d1, d2) => d1 == d2, d => d.GetHashCode(), d => d)
            );

        modelBuilder.Entity<SurveyQuestion>().Property(q => q.Choices)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<string>>((l1, l2) => l1.SequenceEqual(l2), l => l.GetHashCode(), l => l.ToList())
            );

        modelBuilder.Entity<SurveyAnswer>().Property(a => a.Selections)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<HashSet<int>>(v, (JsonSerializerOptions)null),
                new ValueComparer<HashSet<int>>((s1, s2) => s1.SetEquals(s2), s => s.GetHashCode(), s => s.ToHashSet())
            );
    }
}
