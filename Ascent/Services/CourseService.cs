using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class CourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db) { _db = db; }

    public Course GetCourse(int id) => _db.Courses
        .Where(c => c.Id == id)
        .Include(c => c.Coordinators.OrderBy(c => c.Person.LastName)).ThenInclude(c => c.Person)
        .SingleOrDefault();

    public List<Course> GetCourses() => _db.Courses.AsNoTracking()
        .Where(c => !c.IsObsolete)
        .Include(c => c.Coordinators.OrderBy(c => c.Person.LastName)).ThenInclude(c => c.Person)
        .OrderBy(c => c.Number)
        .ToList();

    public List<Course> GetJournalCourses() => _db.Courses.AsNoTracking()
        .Where(c => !c.IsObsolete && c.IsRequired && !c.Number.StartsWith("5"))
        .Include(c => c.CourseJournal).ThenInclude(j => j.Instructor)
        .OrderBy(c => c.Number)
        .ToList();

    public List<Course> SearchCoursesByPrefix(string prefix)
    {
        prefix = prefix?.Trim();
        return _db.Courses.AsNoTracking()
            .Where(c => c.Number.StartsWith(prefix))
            .OrderBy(c => c.Number)
            .ToList();
    }

    public void AddCourse(Course course)
    {
        _db.Courses.Add(course);
        _db.SaveChanges();
    }

    public void AddCoordinator(int id, int personId)
    {
        var coordinator = _db.CourseCoordinators.Where(c => c.CourseId == id && c.PersonId == personId).SingleOrDefault();
        if (coordinator == null)
        {
            _db.CourseCoordinators.Add(new CourseCoordinator()
            {
                CourseId = id,
                PersonId = personId,
            });
            _db.SaveChanges();
        }
    }

    public void RemoveCoordinator(int id, int personId)
    {
        var coordinator = _db.CourseCoordinators.Where(c => c.CourseId == id && c.PersonId == personId).SingleOrDefault();
        if (coordinator != null)
        {
            _db.CourseCoordinators.Remove(coordinator);
            _db.SaveChanges();
        }
    }

    public void SaveChanges() => _db.SaveChanges();
}
