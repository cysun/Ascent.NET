using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class RubricService
{
    private readonly AppDbContext _db;

    public RubricService(AppDbContext db)
    {
        _db = db;
    }

    public Rubric GetRubric(int id) => _db.Rubrics.Find(id);

    public List<Rubric> GetRubrics() => _db.Rubrics.AsNoTracking()
        .Where(s => !s.IsDeleted).OrderByDescending(s => s.TimePublished).ToList();

    public void AddRubric(Rubric rubric)
    {
        _db.Rubrics.Add(rubric);
        _db.SaveChanges();
    }
}
