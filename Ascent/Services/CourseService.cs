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

    // Instead of all course journals, we only want the current ones.
    public List<CourseJournal> GetCourseJournals() => _db.Courses.AsNoTracking()
        .Where(c => c.CourseJournalId != null && !c.IsObsolete)
        .Include(c => c.CourseJournal).ThenInclude(j => j.Instructor)
        .Include(c => c.CourseJournal).ThenInclude(j => j.Course)
        .OrderBy(c => c.Number)
        .Select(c => c.CourseJournal).ToList();

    public List<Course> GetCoursesWithJournal() => _db.Courses.AsNoTracking()
        .Where(c => c.CourseJournalId != null && !c.IsObsolete)
        .OrderBy(c => c.Number)
        .ToList();

    public CourseJournal GetCourseJournal(int id) => _db.CourseJournals.AsNoTracking()
        .Where(j => j.Id == id)
        .Include(j => j.Course).Include(j => j.Instructor).Include(j => j.SampleStudents)
        .SingleOrDefault();

    public void AddCourseJournal(CourseJournal courseJournal)
    {
        var course = GetCourse(courseJournal.CourseId);
        course.CourseJournal = courseJournal;
        _db.CourseJournals.Add(courseJournal);
        _db.SaveChanges();
    }

    public void DeleteCourseJournal(CourseJournal courseJournal)
    {
        _db.CourseJournals.Remove(courseJournal);
        courseJournal.Course.CourseJournalId = null;
        _db.SaveChanges();
    }

    public SampleStudent GetSampleStudent(int id) => _db.SampleStudents.Find(id);

    public void AddSampleStudent(SampleStudent student)
    {
        _db.SampleStudents.Add(student);
        _db.SaveChanges();
    }

    public void RemoveSampleStudent(SampleStudent student)
    {
        _db.SampleStudents.Remove(student);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
