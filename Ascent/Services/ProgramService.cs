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

    public ProgramResource GetResource(int id) => _db.ProgramResources.Find(id);

    public ProgramResource GetResource(int moduleId, int index) => _db.ProgramResources
        .Where(i => i.ModuleId == moduleId && i.Index == index).SingleOrDefault();

    public List<ProgramResource> GetModuleResources(int moduleId) => _db.ProgramResources.AsNoTracking()
        .Where(i => i.ModuleId == moduleId).Include(i => i.File).Include(i => i.Page)
        .OrderBy(i => i.Index).ToList();

    public Dictionary<int, List<ProgramResource>> GetProgramResources(int programId) => _db.ProgramResources.AsNoTracking()
        .Where(i => i.Module.ProgramId == programId).Include(i => i.File).Include(i => i.Page)
        .AsEnumerable().GroupBy(i => i.ModuleId).ToDictionary(g => g.Key, g => g.OrderBy(g => g.Index).ToList());

    public void AddResourceToModule(int moduleId, ProgramResource resource)
    {
        var module = _db.ProgramModules.Find(moduleId);
        if (module == null) return;

        resource.ModuleId = moduleId;
        resource.Index = module.ResourceCount++;
        _db.ProgramResources.Add(resource);
        _db.SaveChanges();
    }

    public void MoveUpResource(ProgramResource resource)
    {
        if (resource.Index > 0)
        {
            var previous = GetResource(resource.ModuleId, resource.Index - 1);
            resource.Index--;
            previous.Index++;
            _db.SaveChanges();
        }
    }

    public void MoveDownResource(ProgramResource resource)
    {
        var module = _db.ProgramModules.Find(resource.ModuleId);
        if (resource.Index < module.ResourceCount - 1)
        {
            var next = GetResource(resource.ModuleId, resource.Index + 1);
            resource.Index++;
            next.Index--;
            _db.SaveChanges();
        }
    }

    public void DeleteResource(ProgramResource resource)
    {
        var module = _db.ProgramModules.Find(resource.ModuleId);
        if (resource.Index < module.ResourceCount - 1)
        {
            var resourcesAfter = _db.ProgramResources.Where(i => i.ModuleId == resource.ModuleId && i.Index > resource.Index);
            foreach (var i in resourcesAfter)
                i.Index--;
            module.ResourceCount--;
        }
        _db.ProgramResources.Remove(resource);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
