using Ascent.Models;
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
        .Where(p => !p.IsDeleted).Include(p => p.Outcomes.OrderBy(o => o.Index)).OrderBy(p => p.Id).ToList();

    public Models.Program GetProgram(int id) => _db.Programs
        .Where(p => p.Id == id).Include(p => p.Outcomes.OrderBy(o => o.Index)).SingleOrDefault();

    public void AddProgram(Models.Program program)
    {
        _db.Programs.Add(program);
        _db.SaveChanges();
    }

    public ProgramModule GetModule(int id) => _db.ProgramModules.Find(id);

    public List<ProgramModule> GetModules(int programId) => _db.ProgramModules.AsNoTracking()
        .Where(m => m.ProgramId == programId).OrderBy(m => m.Index).ToList();

    public void AddModuleToProgram(int programId, ProgramModule module)
    {
        var program = _db.Programs.Find(programId);
        if (program == null) return;

        module.ProgramId = programId;
        module.Index = program.ModuleCount++;
        _db.ProgramModules.Add(module);
        _db.SaveChanges();
    }

    public List<ProgramItem> GetItems(int moduleId) => _db.ProgramItems.AsNoTracking()
        .Where(i => i.ModuleId == moduleId).Include(i => i.File).Include(i => i.Page)
        .OrderBy(i => i.Index).ToList();

    public void AddItemToModule(int moduleId, ProgramItem item)
    {
        var module = _db.ProgramModules.Find(moduleId);
        if (module == null) return;

        item.ModuleId = moduleId;
        item.Index = module.ItemCount++;
        _db.ProgramItems.Add(item);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
