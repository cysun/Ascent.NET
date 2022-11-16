using ImportCSNS.Models;
using Microsoft.EntityFrameworkCore;

namespace ImportCSNS;

public partial class CsnsDbContext : DbContext
{
    private readonly string _connectionString;

    public CsnsDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder.UseNpgsql(_connectionString);

    public virtual DbSet<Models.File> Files { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectResource> ProjectResources { get; set; }
    public virtual DbSet<Resource> Resources { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.File>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("files_pkey");

            entity.ToTable("files");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.Folder).HasColumnName("folder");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Public).HasColumnName("public");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.Regular).HasColumnName("regular");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.SubmissionId).HasColumnName("submission_id");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");

            entity.HasOne(d => d.Owner).WithMany(p => p.Files)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("files_owner_id_fkey");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("files_parent_id_fkey");

            entity.HasOne(d => d.Reference).WithMany(p => p.InverseReference)
                .HasForeignKey(d => d.ReferenceId)
                .HasConstraintName("files_reference_id_fkey");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("projects_pkey");

            entity.ToTable("projects");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description)
                .HasMaxLength(8000)
                .HasColumnName("description");
            entity.Property(e => e.Private).HasColumnName("private");
            entity.Property(e => e.Published).HasColumnName("published");
            entity.Property(e => e.Sponsor)
                .HasMaxLength(255)
                .HasColumnName("sponsor");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasMany(d => d.Advisors).WithMany(p => p.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectAdvisor",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("AdvisorId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("project_advisors_advisor_id_fkey"),
                    l => l.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("project_advisors_project_id_fkey"),
                    j =>
                    {
                        j.HasKey("ProjectId", "AdvisorId").HasName("project_advisors_pkey");
                        j.ToTable("project_advisors");
                    });

            entity.HasMany(d => d.Liaisons).WithMany(p => p.ProjectsNavigation)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectLiaison",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("LiaisonId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("project_liaisons_liaison_id_fkey"),
                    l => l.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("project_liaisons_project_id_fkey"),
                    j =>
                    {
                        j.HasKey("ProjectId", "LiaisonId").HasName("project_liaisons_pkey");
                        j.ToTable("project_liaisons");
                    });

            entity.HasMany(d => d.Students).WithMany(p => p.Projects1)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectStudent",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("project_students_student_id_fkey"),
                    l => l.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("project_students_project_id_fkey"),
                    j =>
                    {
                        j.HasKey("ProjectId", "StudentId").HasName("project_students_pkey");
                        j.ToTable("project_students");
                    });
        });

        modelBuilder.Entity<ProjectResource>(entity =>
        {
            entity.HasKey(e => new { e.ProjectId, e.ResourceOrder }).HasName("project_resources_pkey");

            entity.ToTable("project_resources");

            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ResourceOrder).HasColumnName("resource_order");
            entity.Property(e => e.ResourceId).HasColumnName("resource_id");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectResources)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_resources_project_id_fkey");

            entity.HasOne(d => d.Resource).WithMany(p => p.ProjectResources)
                .HasForeignKey(d => d.ResourceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_resources_resource_id_fkey");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("resources_pkey");

            entity.ToTable("resources");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Deleted).HasColumnName("deleted");
            entity.Property(e => e.FileId).HasColumnName("file_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Private).HasColumnName("private");
            entity.Property(e => e.Text).HasColumnName("text");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Url)
                .HasMaxLength(2000)
                .HasColumnName("url");

            entity.HasOne(d => d.File).WithMany(p => p.Resources)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("resources_file_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Cin, "user_cin_index")
                .IsUnique()
                .HasOperators(new[] { "varchar_pattern_ops" });

            entity.HasIndex(e => e.AccessKey, "users_access_key_key").IsUnique();

            entity.HasIndex(e => e.Cin, "users_cin_key").IsUnique();

            entity.HasIndex(e => e.PrimaryEmail, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AccessKey)
                .HasMaxLength(255)
                .HasColumnName("access_key");
            entity.Property(e => e.CellPhone)
                .HasMaxLength(255)
                .HasColumnName("cell_phone");
            entity.Property(e => e.Cin)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("cin");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("city");
            entity.Property(e => e.DiskQuota)
                .HasDefaultValueSql("200")
                .HasColumnName("disk_quota");
            entity.Property(e => e.Enabled)
                .IsRequired()
                .HasDefaultValueSql("true")
                .HasColumnName("enabled");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.HomePhone)
                .HasMaxLength(255)
                .HasColumnName("home_phone");
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.MajorId).HasColumnName("major_id");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(255)
                .HasColumnName("middle_name");
            entity.Property(e => e.NumOfForumPosts).HasColumnName("num_of_forum_posts");
            entity.Property(e => e.OriginalPictureId).HasColumnName("original_picture_id");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PersonalProgramId).HasColumnName("personal_program_id");
            entity.Property(e => e.PrimaryEmail)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("primary_email");
            entity.Property(e => e.ProfilePictureId).HasColumnName("profile_picture_id");
            entity.Property(e => e.ProfileThumbnailId).HasColumnName("profile_thumbnail_id");
            entity.Property(e => e.SecondaryEmail)
                .HasMaxLength(255)
                .HasColumnName("secondary_email");
            entity.Property(e => e.State)
                .HasMaxLength(255)
                .HasColumnName("state");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("street");
            entity.Property(e => e.Temporary).HasColumnName("temporary");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("username");
            entity.Property(e => e.WorkPhone)
                .HasMaxLength(255)
                .HasColumnName("work_phone");
            entity.Property(e => e.Zip)
                .HasMaxLength(255)
                .HasColumnName("zip");

            entity.HasOne(d => d.OriginalPicture).WithMany(p => p.UserOriginalPictures)
                .HasForeignKey(d => d.OriginalPictureId)
                .HasConstraintName("users_original_picture_id_fkey");

            entity.HasOne(d => d.ProfilePicture).WithMany(p => p.UserProfilePictures)
                .HasForeignKey(d => d.ProfilePictureId)
                .HasConstraintName("users_profile_picture_id_fkey");

            entity.HasOne(d => d.ProfileThumbnail).WithMany(p => p.UserProfileThumbnails)
                .HasForeignKey(d => d.ProfileThumbnailId)
                .HasConstraintName("users_profile_thumbnail_id_fkey");
        });
        modelBuilder.HasSequence("hibernate_sequence").HasMin(2000000L);
        modelBuilder.HasSequence("result_sequence").IsCyclic();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
