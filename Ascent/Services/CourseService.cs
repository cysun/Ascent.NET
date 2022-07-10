using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class CourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db) { _db = db; }

    public Course GetCourse(int id) => _db.Courses.Find(id);

    public List<Course> GetCourses() => _db.Courses.AsNoTracking()
        .Where(c => !c.IsObsolete)
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

    public void SaveChanges() => _db.SaveChanges();
}
