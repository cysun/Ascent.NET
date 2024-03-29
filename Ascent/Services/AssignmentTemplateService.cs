using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class AssignmentTemplateService
{
    private readonly AppDbContext _db;

    public AssignmentTemplateService(AppDbContext db) { _db = db; }

    public AssignmentTemplate GetAssignmentTemplate(int id) => _db.AssignmentTemplates
        .Where(a => a.Id == id).Include(a => a.Rubric)
        .SingleOrDefault();

    public List<AssignmentTemplate> GetAssignmentTemplates() => _db.AssignmentTemplates.AsNoTracking()
        .Include(a => a.Rubric).OrderBy(a => a.Name)
        .ToList();

    public void AddAssignmentTemplate(AssignmentTemplate assignmentTemplate)
    {
        _db.AssignmentTemplates.Add(assignmentTemplate);
        _db.SaveChanges();
    }

    public void DeleteAssignmentTemplate(AssignmentTemplate assignmentTemplate)
    {
        _db.AssignmentTemplates.Remove(assignmentTemplate);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
