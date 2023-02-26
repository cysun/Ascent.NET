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

    // The PostgreSQL implementation of SqlQuery() is a bit odd. Instead of using the provided query directly, it
    // uses the provided query as a subquery in "SELECT t.Value FROM <subquery> AS t". This means the result column
    // of the query must be called "Value".
    public List<int> GetAssessmentYears(int rubricId, int courseId) => _db.Database
        .SqlQuery<int>($@"SELECT DISTINCT ""Year"" AS ""Value"" FROM ""RubricData""
            WHERE ""RubricId"" = {rubricId} AND ""CourseId"" = {courseId}")
        .OrderBy(y => y)
        .ToList();

    public List<AssessmentSection> GetAssessmentSections(int rubricId) => _db.AssessmentSections
        .FromSql($@"SELECT DISTINCT ""CourseId"", ""Term_Code"" FROM ""RubricData"" WHERE ""RubricId"" = {rubricId}")
        .Include(s => s.Course)
        .ToList();

    public List<RubricDataByPerson> GetDataByPerson(int rubricId, int courseId, int fromYear, int toYear) => _db.RubricDataByPerson
    .Where(d => d.RubricId == rubricId && d.CourseId == courseId && fromYear <= d.Year && d.Year <= toYear)
    .ToList();

    public List<RubricDataByPerson> GetDataByPerson(int rubricId, int courseId, int termCode) => _db.RubricDataByPerson
        .Where(d => d.RubricId == rubricId && d.CourseId == courseId && d.TermCode == termCode)
        .ToList();
}
