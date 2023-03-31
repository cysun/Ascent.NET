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

    public List<Survey> GetSurveys() => _db.Surveys.AsNoTracking().Where(s => !s.IsDeleted)
        .OrderByDescending(s => s.TimeClosed).ThenBy(s => s.Name)
        .ToList();

    public void AddSurvey(Survey survey)
    {
        _db.Surveys.Add(survey);
        _db.SaveChanges();
    }

    public Survey CloneSurvey(Survey survey)
    {
        var newSurvey = survey.Clone();
        _db.Surveys.Add(newSurvey);
        _db.SaveChanges();

        var questions = _db.SurveyQuestions.Where(q => q.SurveyId == survey.Id);
        foreach (var question in questions)
            _db.SurveyQuestions.Add(question.Clone(newSurvey.Id));
        _db.SaveChanges();

        return newSurvey;
    }

    public SurveyQuestion GetQuestion(int id) => _db.SurveyQuestions
        .Where(q => q.Id == id).Include(q => q.Survey).SingleOrDefault();

    public SurveyQuestion GetQuestion(int surveyId, int index) => _db.SurveyQuestions
        .Where(q => q.SurveyId == surveyId && q.Index == index).SingleOrDefault();

    public List<SurveyQuestion> GetQuestions(int surveyId) => _db.SurveyQuestions.AsNoTracking()
        .Where(q => q.SurveyId == surveyId).OrderBy(q => q.Index).ToList();

    public void AddQuestionToSurvey(int surveyId, SurveyQuestion question)
    {
        var survey = _db.Surveys.Find(surveyId);
        if (survey == null) return;

        question.SurveyId = surveyId;
        question.Index = survey.QuestionCount++;
        _db.SurveyQuestions.Add(question);
        _db.SaveChanges();
    }

    public async Task DeleteQuestionAsync(int id)
    {
        var question = _db.SurveyQuestions.Find(id);
        if (question == null) return;

        var survey = _db.Surveys.Find(question.SurveyId);
        if (question.Index < survey.QuestionCount - 1)
        {
            await _db.SurveyQuestions
                .Where(q => q.SurveyId == question.SurveyId && q.Index > question.Index)
                .ForEachAsync(q => q.Index--);
        }
        survey.QuestionCount--;
        _db.SurveyQuestions.Remove(question);
        _db.SaveChanges();
    }

    public void AddResponseToSurvey(Survey survey, SurveyResponse response)
    {
        response.Id = Guid.NewGuid();
        response.Survey = survey;
        response.TimeSubmitted = DateTime.UtcNow;
        survey.ResponseCount++;
        _db.SurveyResponses.Add(response);
        _db.SaveChanges();
    }

    public int? DeleteResponse(string id)
    {
        var response = _db.SurveyResponses.Find(Guid.Parse(id));
        if (response == null) return null;

        var survey = _db.Surveys.Find(response.SurveyId);
        survey.ResponseCount--;
        _db.SurveyResponses.Remove(response);
        _db.SaveChanges();
        return survey.Id;
    }

    public List<SurveyAnswer> GetAnswers(int surveyId) => _db.SurveyAnswers.AsNoTracking()
        .Where(a => a.Question.SurveyId == surveyId).ToList();

    public SurveyResponse GetResponse(string id) => _db.SurveyResponses.AsNoTracking()
        .Where(r => r.Id == Guid.Parse(id)).Include(r => r.Survey)
        .Include(r => r.Answers).ThenInclude(a => a.Question).SingleOrDefault();

    public List<SurveyResponse> GetResponses(int surveyId) => _db.SurveyResponses.AsNoTracking()
        .Where(r => r.SurveyId == surveyId && !r.IsDeleted).Include(r => r.Answers.OrderBy(a => a.Question.Index)).ToList();

    public List<SurveyResponse> FindResponses(int questionId, int selection)
    {
        var question = _db.SurveyQuestions.Find(questionId);
        if (question == null) return new List<SurveyResponse>();

        if (question.Type == QuestionType.Rating)
            return _db.SurveyAnswers.AsNoTracking()
                .Where(a => a.QuestionId == questionId && a.Rating == selection)
                .Include(a => a.Response).OrderBy(a => a.Response.TimeSubmitted)
                .Select(a => a.Response).ToList();
        else if (question.Type == QuestionType.Choice && question.MaxSelection == 1)
            return _db.SurveyAnswers.AsNoTracking()
                .Where(a => a.QuestionId == questionId && a.SingleSelection == question.Choices[selection])
                .Include(a => a.Response).OrderBy(a => a.Response.TimeSubmitted)
                .Select(a => a.Response).ToList();
        else if (question.Type == QuestionType.Choice && question.MaxSelection > 1)
        {
            var query = @"SELECT * FROM ""SurveyAnswers"" WHERE ""QuestionId"" = {0}
                AND (""Selections""::json->{1})::text::boolean"; // Need to first cast to text, then to boolean
            return _db.SurveyAnswers.FromSqlRaw(query, questionId, selection).AsNoTracking()
                .Include(a => a.Response).OrderBy(a => a.Response.TimeSubmitted)
                .Select(a => a.Response).ToList();
        }
        else
            return new List<SurveyResponse>();
    }

    public List<OutcomeSurvey> GetOutcomeSurveys() => _db.OutcomeSurveys.AsNoTracking()
        .Include(s => s.Survey).Include(s => s.Program)
        .OrderBy(s => s.Program.Id)
        .ToList();

    public OutcomeSurvey GetOutcomeSurvey(int id) => _db.OutcomeSurveys.Find(id);

    public OutcomeSurvey GetOutcomeSurveyWithProgram(int id) => _db.OutcomeSurveys.AsNoTracking()
        .Where(s => s.Id == id).Include(s => s.Survey)
        .Include(s => s.Program).ThenInclude(p => p.Outcomes)
        .SingleOrDefault();

    public void AddOutcomeSurvey(OutcomeSurvey outcomeSurvey)
    {
        _db.OutcomeSurveys.Add(outcomeSurvey);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
