using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class MftService
{
    private readonly AppDbContext _db;

    public MftService(AppDbContext db) { _db = db; }

    // Scores

    public List<MftScore> GetScores(int year) => _db.MftScores.Where(s => s.Year == year)
        .OrderByDescending(s => s.Score).ThenBy(s => s.FirstName).ThenBy(s => s.LastName).ToList();

    // Score Stats

    public List<MftScoreStat> GetScoreStats() => _db.MftScoreStats.AsNoTracking()
        .OrderByDescending(s => s.Year).ToList();

    // Indicators

    public List<MftIndicator> GetIndicators() => _db.MftIndicators
        .OrderByDescending(i => i.Year).ToList();

    // Distribution Types

    public List<MftDistributionType> GetDistributionTypes() => _db.MftDistributionTypes.AsNoTracking()
        .OrderBy(t => t.Id).ToList();

    // Distributions

    public MftDistribution GetDistribution(int id) => _db.MftDistributions
        .Where(d => d.Id == id).Include(d => d.Type).SingleOrDefault();

    public MftDistribution GetDistribution(int year, string typeAlias) => _db.MftDistributions
        .Where(d => d.Year == year && d.TypeAlias == typeAlias).FirstOrDefault();

    public List<MftDistribution> GetDistributions(int year) => _db.MftDistributions.AsNoTracking()
        .Where(d => d.Year == year).Include(d => d.Type).OrderBy(d => d.Type.Id).ToList();

    public List<int> GetDistributionYears() => _db.MftDistributions.AsNoTracking()
        .Select(d => d.Year).Distinct().OrderByDescending(y => y).ToList();

    public void AddDistribution(MftDistribution distribution)
    {
        _db.MftDistributions.Add(distribution);
        _db.SaveChanges();
    }

    public void DeleteDistribution(int distributionId)
    {
        // One day, EF Core would provide something like DeleteByKey() so we don't have to
        // load an entity before deleting it.
        var distribution = _db.MftDistributions.Find(distributionId);
        _db.MftDistributions.Remove(distribution);
        _db.SaveChanges();
    }
}
