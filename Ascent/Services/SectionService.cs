using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class SectionService
{
    private readonly AppDbContext _db;

    public SectionService(AppDbContext db)
    {
        _db = db;
    }

    public List<Term> GetTerms() => _db.Sections.AsNoTracking()
        .Select(s => s.Term.Code)
        .Distinct()
        .OrderByDescending(c => c)
        .Select(c => new Term(c))
        .ToList();

    public Section GetSection(int id) => _db.Sections
        .Where(s => s.Id == id).Include(s => s.Course).Include(s => s.Instructor)
        .FirstOrDefault();

    public List<Section> GetSections(int termCode) => _db.Sections.AsNoTracking()
        .Where(s => s.Term.Code == termCode).Include(s => s.Course).Include(s => s.Instructor)
        .OrderBy(s => s.Course.Number)
        .ToList();

    public void DeleteSection(Section section)
    {
        _db.Sections.Remove(section);
        _db.SaveChanges();
    }
}
