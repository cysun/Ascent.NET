using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services
{
    public class CourseService
    {
        private readonly AppDbContext _db;

        private readonly ILogger<CourseService> _logger;

        public CourseService(AppDbContext db, ILogger<CourseService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public Course GetCourse(int id) => _db.Courses.Find(id);

        public List<Course> GetCourses() => _db.Courses.Where(c => !c.IsObsolete).OrderBy(c => c.Number).AsNoTracking().ToList();

        public void AddCourse(Course course)
        {
            _db.Courses.Add(course);
            _db.SaveChanges();
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
