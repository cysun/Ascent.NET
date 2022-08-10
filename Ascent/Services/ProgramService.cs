using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class ProgramService
{
    private readonly AppDbContext _db;

    public ProgramService(AppDbContext db)
    {
        _db = db;
    }

    public List<Models.Program> GetPrograms() => _db.Programs.AsNoTracking()
        .Include(p => p.Outcomes.OrderBy(o => o.Index)).OrderBy(p => p.Id).ToList();

    public Models.Program GetProgram(int id) => _db.Programs
        .Where(p => p.Id == id).Include(p => p.Outcomes.OrderBy(o => o.Index)).SingleOrDefault();

    public void AddProgram(Models.Program program)
    {
        _db.Programs.Add(program);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
