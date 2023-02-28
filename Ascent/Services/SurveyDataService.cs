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

    public void DeleteData(OutcomeSurvey outcomeSurvey) => _db.Database
        .ExecuteSql($@"DELETE FROM ""SurveyData"" WHERE ""SurveyId"" = {outcomeSurvey.SurveyId} AND ""ProgramId"" = {outcomeSurvey.ProgramId}
                    AND ""ConstituencyType"" = {outcomeSurvey.ConstituencyType}");

    public void AddData(SurveyDataPoint dataPoint) => _db.SurveyData.Add(dataPoint);

    public void SaveChanges() => _db.SaveChanges();
}
