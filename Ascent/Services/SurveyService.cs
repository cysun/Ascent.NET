using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class SurveyService
{
    private readonly AppDbContext _db;

    public SurveyService(AppDbContext db)
    {
        _db = db;
    }

    public Survey GetSurvey(int id) => _db.Surveys.Find(id);

    public List<Survey> GetSurveys()
    {
        return _db.Surveys.OrderByDescending(s => s.TimeClosed).ThenByDescending(s => s.TimePublished)
            .ThenByDescending(s => s.TimeCreated).ToList();
    }

    public void AddSurvey(Survey survey)
    {
        _db.Surveys.Add(survey);
        _db.SaveChanges();
    }

    public SurveyQuestion GetQuestion(int id) => _db.SurveyQuestions
        .Where(q => q.Id == id).Include(q => q.Survey).SingleOrDefault();

    public List<SurveyQuestion> GetQuestions(int surveyId) => _db.SurveyQuestions
        .Where(q => q.SurveyId == surveyId).OrderBy(q => q.Index).ToList();

    public void AddQuestionToSurvey(int surveyId, SurveyQuestion question)
    {
        var survey = _db.Surveys.Find(surveyId);
        if (survey == null) return;

        question.SurveyId = surveyId;
        question.Index = survey.NumOfQuestions++;
        _db.SurveyQuestions.Add(question);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
