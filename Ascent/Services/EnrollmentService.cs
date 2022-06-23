using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class EnrollmentService
{
    private readonly AppDbContext _db;

    public EnrollmentService(AppDbContext db)
    {
        _db = db;
    }

    public List<Enrollment> GetEnrollmentsByPerson(int personId) =>
        _db.Enrollments.AsNoTracking().Where(e => e.StudentId == personId)
        .Include(e => e.Section).ThenInclude(s => s.Course)
        .Include(e => e.Section).ThenInclude(s => s.Instructor)
        .OrderByDescending(e => e.Section.Term.Code).ToList();

    public List<Enrollment> GetEnrollmentsBySection(int sectionId) =>
        _db.Enrollments.AsNoTracking().Where(e => e.SectionId == sectionId)
        .Include(e => e.Student)
        .AsEnumerable().OrderBy(e => e.Student.FullName)
        .ToList();
}
