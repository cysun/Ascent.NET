using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class RubricDataService
{
    private readonly AppDbContext _db;

    public RubricDataService(AppDbContext db)
    {
        _db = db;
    }

    public List<AssessmentSection> GetAssessmentSections(int rubricId) => _db.AssessmentSections
        .FromSql($@"SELECT DISTINCT ""CourseId"", ""Term_Code"" FROM ""RubricData"" WHERE ""RubricId"" = {rubricId}")
        .Include(s => s.Course)
        .ToList();

    public List<RubricDataByPerson> GetDataByPerson(int rubricId, int courseId, int termCode) => _db.RubricDataByPerson
        .Where(d => d.RubricId == rubricId && d.CourseId == courseId && d.TermCode == termCode)
        .ToList();
}
