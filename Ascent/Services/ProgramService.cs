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

    public ProgramModule GetModule(int programId, int index) => _db.ProgramModules
        .Where(m => m.ProgramId == programId && m.Index == index).SingleOrDefault();

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

    public void MoveUpModule(ProgramModule module)
    {
        if (module.Index > 0)
        {
            var previous = GetModule(module.ProgramId, module.Index - 1);
            module.Index--;
            previous.Index++;
            _db.SaveChanges();
        }
    }

    public void MoveDownModule(ProgramModule module)
    {
        var program = _db.Programs.Find(module.ProgramId);
        if (module.Index < program.ModuleCount - 1)
        {
            var next = GetModule(module.ProgramId, module.Index + 1);
            module.Index++;
            next.Index--;
            _db.SaveChanges();
        }
    }

    public void DeleteModule(ProgramModule module)
    {
        var program = _db.Programs.Find(module.ProgramId);
        if (module.Index < program.ModuleCount - 1)
        {
            var modulesAfter = _db.ProgramModules.Where(m => m.ProgramId == module.ProgramId && m.Index > module.Index);
            foreach (var m in modulesAfter)
                m.Index--;
            program.ModuleCount--;
        }
        _db.ProgramModules.Remove(module);
        _db.SaveChanges();
    }

    public ProgramItem GetItem(int id) => _db.ProgramItems.Find(id);

    public ProgramItem GetItem(int moduleId, int index) => _db.ProgramItems
        .Where(i => i.ModuleId == moduleId && i.Index == index).SingleOrDefault();

    public List<ProgramItem> GetModuleItems(int moduleId) => _db.ProgramItems.AsNoTracking()
        .Where(i => i.ModuleId == moduleId).Include(i => i.File).Include(i => i.Page)
        .OrderBy(i => i.Index).ToList();

    public Dictionary<int, List<ProgramItem>> GetProgramItems(int programId) => _db.ProgramItems.AsNoTracking()
        .Where(i => i.Module.ProgramId == programId).Include(i => i.File).Include(i => i.Page)
        .AsEnumerable().GroupBy(i => i.ModuleId).ToDictionary(g => g.Key, g => g.OrderBy(g => g.Index).ToList());

    public void AddItemToModule(int moduleId, ProgramItem item)
    {
        var module = _db.ProgramModules.Find(moduleId);
        if (module == null) return;

        item.ModuleId = moduleId;
        item.Index = module.ItemCount++;
        _db.ProgramItems.Add(item);
        _db.SaveChanges();
    }

    public void MoveUpItem(ProgramItem item)
    {
        if (item.Index > 0)
        {
            var previous = GetItem(item.ModuleId, item.Index - 1);
            item.Index--;
            previous.Index++;
            _db.SaveChanges();
        }
    }

    public void MoveDownItem(ProgramItem item)
    {
        var module = _db.ProgramModules.Find(item.ModuleId);
        if (item.Index < module.ItemCount - 1)
        {
            var next = GetItem(item.ModuleId, item.Index + 1);
            item.Index++;
            next.Index--;
            _db.SaveChanges();
        }
    }

    public void DeleteItem(ProgramItem item)
    {
        var module = _db.ProgramModules.Find(item.ModuleId);
        if (item.Index < module.ItemCount - 1)
        {
            var itemsAfter = _db.ProgramItems.Where(i => i.ModuleId == item.ModuleId && i.Index > item.Index);
            foreach (var i in itemsAfter)
                i.Index--;
            module.ItemCount--;
        }
        _db.ProgramItems.Remove(item);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
