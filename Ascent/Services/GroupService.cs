using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class GroupService
{
    private readonly AppDbContext _db;

    private readonly ILogger<GroupService> _logger;

    public GroupService(AppDbContext db, ILogger<GroupService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public List<Group> GetGroups() => _db.Groups.AsNoTracking()
        .OrderBy(g => g.Id).ToList();

    public Group GetGroup(int id) => _db.Groups.Find(id);

    public void AddGroup(Group group)
    {
        _db.Groups.Add(group);
        _db.SaveChanges();
    }

    public void DeleteGroup(Group group)
    {
        _db.Groups.Remove(group);
        _db.SaveChanges();
    }

    public List<Person> GetMembers(Group group)
    {
        if (!group.IsVirtual)
            return _db.GroupMembers.AsNoTracking()
                .Where(m => m.GroupId == group.Id).Include(m => m.Person)
                .Select(m => m.Person).OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                .ToList();

        switch (group.Name)
        {
            case "BS Alumni":
                return _db.GroupMembers.AsNoTracking().Where(m => m.GroupId == group.Id).Include(m => m.Person)
                    .Select(m => m.Person).Where(p => p.BgTerm != null && string.IsNullOrWhiteSpace(p.PersonalEmail))
                    .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                    .ToList();

            case "MS Alumni":
                return _db.GroupMembers.AsNoTracking().Where(m => m.GroupId == group.Id).Include(m => m.Person)
                    .Select(m => m.Person).Where(p => p.BgTerm != null && string.IsNullOrWhiteSpace(p.PersonalEmail))
                    .OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
                    .ToList();

            default:
                _logger.LogWarning("Unrecognized virtual group name: {group}", group.Name);
                return new List<Person>();
        }
    }

    public void SaveChanges() => _db.SaveChanges();
}
