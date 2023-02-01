using Ascent.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ImportCSNS;

public partial class Importer
{
    public void ImportRubricData()
    {
        // Connect to two databases
        // See https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ on how to create DbContext

        using var csnsDb = new CsnsDbContext(_config.GetConnectionString("CsnsConnection"));

        var appDbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_config.GetConnectionString("DefaultConnection"))
            .Options;
        using var ascentDb = new AppDbContext(appDbContextOptions);

        var crubrics = csnsDb.Rubrics.Where(r => r.IsPublic && !r.IsOsolete && !r.IsDeleted)
            .Include(r => r.Indicators.OrderBy(i => i.Index)).ThenInclude(i => i.Criteria.OrderBy(c => c.Index))
            .ToList();

        foreach (var crubric in crubrics)
        {
            Console.WriteLine($"{crubric.Name}");
            foreach (var indicator in crubric.Indicators)
                Console.WriteLine($"\t{indicator.Name}, {indicator.Criteria.Count} criteria");
        }

        var csections = csnsDb.Sections
            .Where(s => !s.IsDeleted && s.Course.Code.StartsWith("CS"))
            .Include(s => s.Course)
            .OrderByDescending(s => s.TermCode)
            .ToList();

        foreach (var csection in csections)
            Console.WriteLine($"{csection.TermCode} {csection.Course.Code} {csection.Course.Name}");

        var cevaluations = csnsDb.RubricEvaluations.AsNoTracking()
            .Where(e => e.IsCompleted && !e.IsDeleted)
            .Include(e => e.Submission).ThenInclude(s => s.Assignment).ThenInclude(a => a.Section)
            .ToList();

        Console.WriteLine($"{cevaluations.Count} rubric evaluations.");
    }
}
