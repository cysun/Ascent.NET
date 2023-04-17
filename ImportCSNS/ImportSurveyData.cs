using Ascent.Models;
using Ascent.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ImportCSNS;

public partial class Importer
{
    public void ImportSurveyData()
    {
        var surveysToImport = new Dictionary<long, int>
        {
            { 7398788, 2 },     // 2018-2020 CSULA Computer Science Undergraduate Program Survey - IAB
            { 7409972, 10 },    // 2018-2020 CSULA Computer Science Undergraduate Program Survey - Faculty
            { 7829162, 11 },    // 2020-2022 CSULA Computer Science Undergraduate Program Survey - Student
            { 7825459, 2 },     // 2020-2022 CSULA Computer Science Undergraduate Program Survey - IAB
            { 7829172, 10 },    // 2020-2022 CSULA Computer Science Undergraduate Program Survey - Faculty
            { 7829182, 9 },     // 2020-2022 CSULA Computer Science Undergraduate Program Survey - Alumni

            { 5898784, 8 },     // 2018-2020 CSULA Computer Science Graduate Program Survey - Student
            { 7377372, 3 },     // 2018-2020 CSULA Computer Science Graduate Program Survey - IAB
            { 7377015, 7 },     // 2018-2020 CSULA Computer Science Graduate Program Survey - Faculty
            { 7363720, 6 },     // 2018-2020 CSULA Computer Science Graduate Program Survey - Alumni
            { 7829134, 8 },     // 2020-2022 CSULA Computer Science Graduate Program Survey - Student
            { 7825449, 3 },     // 2020-2022 CSULA Computer Science Graduate Program Survey - IAB
            { 7829142, 7 },     // 2020-2022 CSULA Computer Science Graduate Program Survey - Faculty
            { 7829150, 6 }      // 2020-2022 CSULA Computer Science Graduate Program Survey - Alumni
        };

        // Connect to two databases
        // See https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ on how to create DbContext

        using var csnsDb = new CsnsDbContext(_config.GetConnectionString("CsnsConnection"));

        var appDbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_config.GetConnectionString("DefaultConnection"))
            .Options;
        using var ascentDb = new AppDbContext(appDbContextOptions);

        foreach (var cSurveyId in surveysToImport.Keys)
        {
            var aSurvey = ascentDb.Surveys.Find(surveysToImport[cSurveyId]);
            var aQuestions = ascentDb.SurveyQuestions.AsNoTracking()
                .Where(q => q.SurveyId == aSurvey.Id).OrderBy(q => q.Index)
                .ToList();

            var cResponses = csnsDb.SurveyResponses.AsNoTracking()
                .Where(r => r.SurveyId == cSurveyId)
                .Include(r => r.AnswerSheet).ThenInclude(s => s.Sections)
                .ThenInclude(s => s.Answers.OrderBy(a => a.AnswerIndex))
                .ToList();

            foreach (var cResponse in cResponses)
            {
                var aResponse = new SurveyResponse
                {
                    Id = Guid.NewGuid(),
                    Survey = aSurvey,
                    TimeSubmitted = cResponse.AnswerSheet.Date.ToUniversalTime(),
                };

                for (int i = 0; i < cResponse.AnswerSheet.Sections[0].Answers.Count; ++i)
                {
                    var aQuestion = aQuestions[i];
                    var cAnswer = cResponse.AnswerSheet.Sections[0].Answers[i];

                    var aAnswer = new SurveyAnswer
                    {
                        QuestionId = aQuestions[i].Id
                    };

                    if (aQuestion.Type == QuestionType.Choice && cAnswer.Type == Models.QuestionType.Rating)
                    {
                        aAnswer.SingleSelection = aQuestion.Choices[cAnswer.Rating - 1];
                    }
                    else if (aQuestion.Type == QuestionType.Text && cAnswer.Type == Models.QuestionType.Text)
                    {
                        aAnswer.Text = cAnswer.Text;
                    }
                    else
                    {
                        Console.WriteLine("\t Something is wrong");
                    }

                    aResponse.Answers.Add(aAnswer);
                }

                aSurvey.ResponseCount++;
                ascentDb.SurveyResponses.Add(aResponse);
                ascentDb.SaveChanges();
            }

            Console.WriteLine($"{cSurveyId}: {cResponses.Count} responses");
        }
    }
}
