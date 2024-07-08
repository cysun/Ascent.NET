using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class ProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db) { _db = db; }

    public List<string> GetAcademicYears() => _db.Projects.AsNoTracking()
        .Where(p => !p.IsDeleted).Select(p => p.AcademicYear).Distinct().OrderByDescending(y => y).ToList();

    public List<Project> GetProjects(string academicYear) => _db.Projects.AsNoTracking()
        .Where(p => p.AcademicYear == academicYear && !p.IsDeleted)
        .Include(p => p.Members.OrderBy(s => s.Person.LastName)).ThenInclude(s => s.Person)
        .OrderBy(p => p.Title).ToList();

    public List<Project> GetProjectsByMember(int memberId, Project.MemberType memberType) => _db.Projects.AsNoTracking()
        .Where(p => p.Members.Any(m => m.PersonId == memberId && m.Type == memberType && !p.IsDeleted))
        .OrderByDescending(p => p.AcademicYear).ToList();

    public Project GetProject(int id) => _db.Projects.Find(id);

    public Project GetProjectWithMembers(int id) => _db.Projects.AsNoTracking()
        .Where(p => p.Id == id).Include(p => p.Members.OrderBy(s => s.Person.LastName)).ThenInclude(s => s.Person)
        .SingleOrDefault();

    public Project GetProjectWithResources(int id) => _db.Projects.AsNoTracking()
        .Where(p => p.Id == id).Include(p => p.Resources.OrderBy(r => r.Name))
        .SingleOrDefault();

    public Project GetFullProject(int id) => _db.Projects.AsNoTracking()
        .Where(p => p.Id == id)
        .Include(p => p.Members.OrderBy(s => s.Person.LastName)).ThenInclude(s => s.Person)
        .Include(p => p.Resources.OrderBy(r => r.Name))
        .SingleOrDefault();

    public void AddProject(Project project)
    {
        _db.Projects.Add(project);
        _db.SaveChanges();
    }

    public void AddProjectMember(int id, int personId, string memberType)
    {
        var ok = Enum.TryParse(memberType, out Project.MemberType type);
        if (!ok) return;

        var member = _db.ProjectMembers.Where(m => m.ProjectId == id && m.PersonId == personId && m.Type == type).SingleOrDefault();
        if (member == null)
        {
            _db.ProjectMembers.Add(new ProjectMember()
            {
                ProjectId = id,
                PersonId = personId,
                Type = type
            });
            _db.SaveChanges();
        }
    }

    public void RemoveProjectMember(int id, int personId, string memberType)
    {
        var ok = Enum.TryParse(memberType, out Project.MemberType type);
        if (!ok) return;

        var member = _db.ProjectMembers.Where(m => m.ProjectId == id && m.PersonId == personId && m.Type == type).SingleOrDefault();
        if (member != null)
        {
            _db.ProjectMembers.Remove(member);
            _db.SaveChanges();
        }
    }

    public ProjectResource GetProjectResource(int id) => _db.ProjectResources
        .Where(r => r.Id == id).Include(r => r.Project)
        .SingleOrDefault();

    public void AddProjectResource(ProjectResource resource)
    {
        _db.ProjectResources.Add(resource);
        _db.SaveChanges();
    }

    public void RemoveProjectResource(ProjectResource resource)
    {
        _db.ProjectResources.Remove(resource);
        _db.SaveChanges();
    }

    // maxResults=null for unlimited results
    public List<Project> SearchProjects(string searchText, int? maxResults = null)
    {
        if (string.IsNullOrWhiteSpace(searchText)) return new List<Project>();

        return _db.Projects.FromSqlRaw("SELECT * FROM \"SearchProjects\"({0}, {1})", searchText, maxResults)
            .OrderByDescending(p => p.AcademicYear)
            .AsNoTracking().ToList();
    }

    public void SaveChanges() => _db.SaveChanges();
}
