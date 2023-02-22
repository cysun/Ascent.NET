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

    //    public List<Section> GetAssessmentSections(int rubricId) => _db.Sections
    //        .FromSql($@"SELECT DISTINCT ""Term_Code"" AS ""Term"", ""CourseId"", 1 As ""InstructorId"",
    //                    DENSE_RANK() OVER (ORDER BY ""CourseId"", ""Term_Code"") AS ""Id""
    //                    FROM ""RubricData"" WHERE ""RubricId"" = {rubricId}")
    //        .FromSql($@"select * from ""Sections""")
    //        .Include(s => s.Course)
    //        .AsNoTracking().ToList();

    public List<AssessmentSection> GetAssessmentSections(int rubricId) => _db.AssessmentSections
        .FromSql($@"SELECT DISTINCT ""CourseId"", ""Term_Code"" FROM ""RubricData"" WHERE ""RubricId"" = {rubricId}")
        .Include(s => s.Course)
        .ToList();
}
