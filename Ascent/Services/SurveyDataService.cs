using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class SurveyDataService
{
    private readonly AppDbContext _db;

    public SurveyDataService(AppDbContext db)
    {
        _db = db;
    }

    public List<int> GetSurveyYears(int programId) => _db.Database
        .SqlQuery<int>($@"SELECT DISTINCT ""Year"" AS ""Value"" FROM ""SurveyData"" WHERE ""ProgramId"" = {programId}")
        .OrderBy(y => y)
        .ToList();

    public List<SurveyDataPoint> GetSurveyData(int programId, int year) => _db.SurveyData.AsNoTracking()
        .Where(d => d.ProgramId == programId && d.Year == year)
        .ToList();

    // Unlike all the other AddXXX() methods, this one does NOT do _db.SaveChanges() so we can batch all the inserts together.
    public void AddData(SurveyDataPoint dataPoint) => _db.SurveyData.Add(dataPoint);

    public void DeleteData(OutcomeSurvey outcomeSurvey) => _db.Database
        .ExecuteSql($@"DELETE FROM ""SurveyData"" WHERE ""SurveyId"" = {outcomeSurvey.SurveyId} AND ""ProgramId"" = {outcomeSurvey.ProgramId}
                    AND ""ConstituencyType"" = {outcomeSurvey.ConstituencyType}");

    public void SaveChanges() => _db.SaveChanges();
}
