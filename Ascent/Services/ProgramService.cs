namespace Ascent.Services;

public class ProgramService
{
    private readonly AppDbContext _db;

    public ProgramService(AppDbContext db)
    {
        _db = db;
    }

    public List<Models.Program> GetPrograms() => _db.Programs.OrderBy(p => p.Name).ToList();

    public Models.Program GetProgram(int id) => _db.Programs.Find(id);

    public void AddProgram(Models.Program program)
    {
        _db.Programs.Add(program);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
