using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class MftService
{
    private readonly AppDbContext _db;

    public MftService(AppDbContext db) { _db = db; }

    public List<DateOnly> GetScoreDates() => _db.MftScores.AsNoTracking()
        .Select(s => s.Date).Distinct().OrderByDescending(d => d)
        .ToList();

    public List<MftScore> GetScores(DateOnly date) => _db.MftScores.AsNoTracking()
        .Where(s => s.Date == date).OrderByDescending(s => s.Score)
        .ToList();
}
