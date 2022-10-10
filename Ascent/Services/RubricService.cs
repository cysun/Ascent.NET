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

    public RubricCriterion GetCriterion(int id) => _db.RubricCriteria
        .Where(c => c.Id == id).Include(c => c.Rubric).Include(c => c.Ratings.OrderBy(r => r.Index))
        .SingleOrDefault();

    public List<RubricCriterion> GetCriteria(int rubricId) => _db.RubricCriteria.AsNoTracking()
        .Where(c => c.RubricId == rubricId).Include(c => c.Ratings.OrderBy(r => r.Index))
        .OrderBy(c => c.Index).ToList();

    public void AddCriterionToRubric(int rubricId, RubricCriterion criterion)
    {
        var rubric = _db.Rubrics.Find(rubricId);
        if (rubric == null) return;

        criterion.RubricId = rubricId;
        criterion.Index = rubric.CriteriaCount++;
        _db.RubricCriteria.Add(criterion);
        _db.SaveChanges();
    }

    public async Task DeleteCriterionAsync(int id)
    {
        var criterion = _db.RubricCriteria.Find(id);
        if (criterion == null) return;

        var rubric = _db.Rubrics.Find(criterion.RubricId);
        if (criterion.Index < rubric.CriteriaCount - 1)
        {
            await _db.RubricCriteria
                .Where(c => c.RubricId == criterion.RubricId && c.Index > criterion.Index)
                .ForEachAsync(q => q.Index--);
        }
        rubric.CriteriaCount--;
        _db.RubricCriteria.Remove(criterion);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
