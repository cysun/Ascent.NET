using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class CourseTemplateService
{
    private readonly AppDbContext _db;

    public CourseTemplateService(AppDbContext db) { _db = db; }

    public CourseTemplate GetCourseTemplate(int id) => _db.CourseTemplates
        .Where(t => t.Id == id)
        .Include(t => t.Course)
        .SingleOrDefault();

    public List<CourseTemplate> GetCourseTemplates() => _db.CourseTemplates.AsNoTracking()
        .Include(t => t.Course).Include(t => t.AssignmentTemplates)
        .OrderBy(t => t.Course.Subject).ThenBy(t => t.Course.Number)
        .ToList();

    public void AddCourseTemplate(CourseTemplate courseTemplate)
    {
        _db.CourseTemplates.Add(courseTemplate);
        _db.SaveChanges();
    }

    public bool IsCourseTemplateExists(int courseId) => _db.CourseTemplates.Any(t => t.CourseId == courseId);

    public void SaveChanges() => _db.SaveChanges();
}
