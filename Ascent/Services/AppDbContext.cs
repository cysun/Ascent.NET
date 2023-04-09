using System.Text.Json;
using Ascent.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Ascent.Services;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseJournal> CourseJournals { get; set; }
    public DbSet<SampleStudent> SampleStudents { get; set; }
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
    public DbSet<Models.Program> Programs { get; set; }
    public DbSet<ProgramOutcome> ProgramOutcomes { get; set; }
    public DbSet<ProgramModule> ProgramModules { get; set; }
    public DbSet<ProgramResource> ProgramResources { get; set; }
    public DbSet<Rubric> Rubrics { get; set; }
    public DbSet<RubricCriterion> RubricCriteria { get; set; }
    public DbSet<RubricRating> RubricRatings { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<ProjectResource> ProjectResources { get; set; }
    public DbSet<RubricDataPoint> RubricData { get; set; }
    public DbSet<RubricDataByPerson> RubricDataByPerson { get; set; }
    public DbSet<AssessmentSection> AssessmentSections { get; set; }
    public DbSet<OutcomeSurvey> OutcomeSurveys { get; set; }
    public DbSet<SurveyDataPoint> SurveyData { get; set; }
    public DbSet<CourseTemplate> CourseTemplates { get; set; }
    public DbSet<AssignmentTemplate> AssignmentTemplates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>().HasIndex(p => p.CampusId).IsUnique();
        modelBuilder.Entity<Group>().HasIndex(g => g.Name).IsUnique();
        modelBuilder.Entity<Group>().Property(g => g.EmailPreference).HasConversion<string>();
        modelBuilder.Entity<GroupMember>().HasKey(m => new { m.GroupId, m.PersonId });
        modelBuilder.Entity<Course>().HasAlternateKey(c => new { c.Subject, c.Number });
        modelBuilder.Entity<Course>().HasOne(c => c.CourseJournal).WithOne();
        modelBuilder.Entity<CourseJournal>().HasOne(j => j.Course).WithMany();
        modelBuilder.Entity<Enrollment>().HasAlternateKey(e => new { e.SectionId, e.StudentId });
        modelBuilder.Entity<PageRevision>().HasKey(r => new { r.PageId, r.Version });
        modelBuilder.Entity<FileRevision>().HasKey(r => new { r.FileId, r.Version });
        modelBuilder.Entity<MftScore>().HasAlternateKey(s => new { s.Year, s.StudentId });
        modelBuilder.Entity<MftIndicator>().Property(i => i.Percentiles).HasDefaultValueSql("'{null, null, null}'");
        modelBuilder.Entity<MftDistributionType>().HasAlternateKey(t => t.Alias);
        modelBuilder.Entity<MftDistribution>().HasAlternateKey(d => new { d.Year, d.TypeAlias });
        modelBuilder.Entity<MftDistribution>().HasOne(d => d.Type).WithMany()
            .HasForeignKey(d => d.TypeAlias).HasPrincipalKey(t => t.Alias);
        modelBuilder.Entity<SurveyQuestion>().HasIndex(q => new { q.SurveyId, q.Index });
        modelBuilder.Entity<SurveyQuestion>().Property(q => q.Type).HasConversion<string>();
        modelBuilder.Entity<SurveyAnswer>().HasAlternateKey(a => new { a.ResponseId, a.QuestionId });
        modelBuilder.Entity<ProgramOutcome>().HasIndex(o => new { o.ProgramId, o.Index });
        modelBuilder.Entity<ProgramModule>().HasIndex(m => new { m.ProgramId, m.Index });
        modelBuilder.Entity<ProgramResource>().Property(i => i.Type).HasConversion<string>();
        modelBuilder.Entity<ProgramResource>().HasIndex(i => new { i.ModuleId, i.Index });
        modelBuilder.Entity<ProjectResource>().Property(i => i.Type).HasConversion<string>();
        modelBuilder.Entity<RubricDataByPerson>().ToView("RubricDataByPerson");

        // The following views do not exist, but without these EF Core will create tables.
        modelBuilder.Entity<AssessmentSection>().ToView("AssessmentSesions");

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
                v => JsonSerializer.Deserialize<List<bool>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<bool>>((l1, l2) => l1.SequenceEqual(l2), l => l.GetHashCode(), l => l.ToList())
            );

        modelBuilder.Entity<Models.Program>().Property(p => p.Objectives)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null),
                new ValueComparer<List<string>>((l1, l2) => l1.SequenceEqual(l2), l => l.GetHashCode(), l => l.ToList())
        );
    }
}
