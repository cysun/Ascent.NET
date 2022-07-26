using Ascent.Models;

namespace Ascent.Services;

public class SurveyService
{
    private readonly AppDbContext _db;

    public SurveyService(AppDbContext db)
    {
        _db = db;
    }

    public List<Survey> GetSurveys()
    {
        return _db.Surveys.OrderByDescending(s => s.TimeClosed).ThenByDescending(s => s.TimePublished)
            .ThenByDescending(s => s.TimeCreated).ToList();
    }

    public void SaveChanges() => _db.SaveChanges();
}
