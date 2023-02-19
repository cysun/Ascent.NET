using Ascent.Models;
using Ascent.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ImportCSNS;

public partial class Importer
{
    public void ImportRubricData()
    {
        var rubricsToImport = new Dictionary<long, int>
        {
            { 7886385, 1 }, // Software Engineering - Analysis (Ver 2.0)
            { 7903926, 2 }, // Software Engineering - Design, Implementation, and Evaluation
            { 7866858, 3 }, // Written Communication (Ver 3.0)
            { 7886395, 4 }, // Oral Communication (Ver 3.0)
            { 7899224, 5 }, // Ethics in the Computer Age (Ver 4.0)
            { 7903933, 6 }, // Teamwork (Ver 3.0)
            { 7899219, 7 } // Knowledge Outcomes (Ver 3.0)
        };

        // Connect to two databases
        // See https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ on how to create DbContext

        using var csnsDb = new CsnsDbContext(_config.GetConnectionString("CsnsConnection"));

        var appDbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_config.GetConnectionString("DefaultConnection"))
            .Options;
        using var ascentDb = new AppDbContext(appDbContextOptions);

        foreach (var crubricId in rubricsToImport.Keys)
        {
            var arubric = ascentDb.Rubrics.AsNoTracking()
                .Where(r => r.Id == rubricsToImport[crubricId])
                .Include(r => r.Criteria.OrderBy(c => c.Index)).ThenInclude(c => c.Ratings.OrderBy(r => r.Index))
                .SingleOrDefault();
            if (arubric == null)
            {
                Console.WriteLine($"Cannot find arubric with Id={rubricsToImport[crubricId]}");
                continue;
            }

            var cevaluations = csnsDb.RubricEvaluations.AsNoTracking()
                .Where(e => e.IsCompleted && !e.IsDeleted && e.Submission.Assignment.RubricId == crubricId)
                .Include(e => e.Ratings.OrderBy(r => r.Index))
                .Include(e => e.Evaluator)
                .Include(e => e.Submission).ThenInclude(s => s.Student)
                .Include(e => e.Submission).ThenInclude(s => s.Assignment).ThenInclude(a => a.Section).ThenInclude(s => s.Course)
                .ToList();

            int pointCount = 0;

            foreach (var cevaluation in cevaluations)
            {
                var evaluator = ascentDb.Persons.Where(p => p.CampusId == cevaluation.Evaluator.Cin).SingleOrDefault();
                if (evaluator == null)
                {
                    Console.WriteLine($"Cannot find evaluator with CIN={cevaluation.Evaluator.Cin}");
                    continue;
                }

                var evaluatee = ascentDb.Persons.Where(p => p.CampusId == cevaluation.Submission.Student.Cin).SingleOrDefault();
                if (evaluatee == null)
                {
                    Console.WriteLine($"Cannot find evaluatee with CIN={cevaluation.Evaluator.Cin}");
                    continue;
                }

                if (cevaluation.Ratings.Count != arubric.CriteriaCount)
                {
                    Console.WriteLine($"Wrong rating/criteria count for rubric {arubric.Id}");
                    continue;
                }

                var isEvalIncomplete = false;
                foreach (var rating in cevaluation.Ratings)
                    if (rating.Rating == -1)
                    {
                        isEvalIncomplete = true;
                        break;
                    }
                if (isEvalIncomplete)
                {
                    Console.WriteLine($"Incomplete rating in evaluation {cevaluation.Id}");
                    continue;
                }

                var acourse = ascentDb.Courses.AsNoTracking()
                    .Where(c => c.Subject + c.Number == cevaluation.Submission.Assignment.Section.Course.Code)
                    .SingleOrDefault();
                if (acourse == null)
                {
                    Console.WriteLine($"Cannot find course {cevaluation.Submission.Assignment.Section.Course.Code}");
                    continue;
                }

                for (int i = 0; i < cevaluation.Ratings.Count; i++)
                {
                    var dataPoint = new RubricDataPoint
                    {
                        RubricId = arubric.Id,
                        CriterionId = arubric.Criteria[i].Id,
                        RatingId = arubric.Criteria[i].Ratings[cevaluation.Ratings[i].Rating - 1].Id,
                        EvaluatorId = evaluator.Id,
                        EvaluateeId = evaluatee.Id,
                        AssessmentType = cevaluation.Type == "PEER" ? RubricAssessmentType.Peer : RubricAssessmentType.Instructor,
                        Term = new Term(cevaluation.Submission.Assignment.Section.TermCode),
                        CourseId = acourse.Id
                    };

                    ascentDb.RubricData.Add(dataPoint);
                    ++pointCount;
                }
            }
            ascentDb.SaveChanges();
            Console.WriteLine($"{pointCount} data points imported");
        }
    }
}
