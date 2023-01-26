using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class ProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db) { _db = db; }

    public List<string> GetAcademicYears() => _db.Projects.AsNoTracking()
        .Select(p => p.AcademicYear).Distinct().OrderByDescending(y => y).ToList();

    public List<Project> GetProjects(string academicYear) => _db.Projects.AsNoTracking()
        .Where(p => p.AcademicYear == academicYear)
        .Include(p => p.Students.OrderBy(s => s.Person.LastName)).ThenInclude(s => s.Person)
        .Include(p => p.Advisors.OrderBy(a => a.Person.LastName)).ThenInclude(a => a.Person)
        .OrderBy(p => p.Title).ToList();

    public Project GetProject(int id) => _db.Projects.Find(id);

    public Project GetFullProject(int id) => _db.Projects.AsNoTracking()
        .Where(p => p.Id == id)
        .Include(p => p.Students.OrderBy(s => s.Person.LastName)).ThenInclude(s => s.Person)
        .Include(p => p.Advisors.OrderBy(a => a.Person.LastName)).ThenInclude(a => a.Person)
        .Include(p => p.Liaisons.OrderBy(l => l.Person.LastName)).ThenInclude(l => l.Person)
        .Include(p => p.Items)
        .SingleOrDefault();

    public void AddProject(Project project)
    {
        _db.Projects.Add(project);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
