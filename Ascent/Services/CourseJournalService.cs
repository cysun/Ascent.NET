using Ascent.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class CourseJournalService
{
    private readonly AppDbContext _db;

    private readonly IMapper _mapper;

    public CourseJournalService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public List<CourseJournal> GetCourseJournals() => _db.CourseJournals.AsNoTracking()
        .Where(j => !j.Course.IsObsolete)
        .Include(j => j.Course).Include(j => j.Instructor)
        .OrderBy(j => j.Course.Number)
        .ToList();

    public CourseJournal GetCourseJournal(int id) => _db.CourseJournals
        .Where(j => j.Id == id)
        .Include(j => j.Course).Include(j => j.Instructor)
        .SingleOrDefault();

    public CourseJournal AddOrUpdateCourseJournal(CourseJournal courseJournal)
    {
        var currentCourseJournal = _db.CourseJournals.Where(j => j.CourseId == courseJournal.CourseId).SingleOrDefault();
        if (currentCourseJournal == null)
        {
            _db.CourseJournals.Add(courseJournal);
            _db.SaveChanges();
            return courseJournal;
        }
        else
        {
            _mapper.Map(courseJournal, currentCourseJournal);
            _db.SaveChanges();
            return currentCourseJournal;
        }
    }

    public void DeleteCourseJournal(CourseJournal courseJournal)
    {
        _db.CourseJournals.Remove(courseJournal);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
