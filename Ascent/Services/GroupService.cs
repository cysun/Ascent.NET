using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class GroupService
{
    private readonly AppDbContext _db;

    public GroupService(AppDbContext db) { _db = db; }

    public List<Group> GetGroups() => _db.Groups.AsNoTracking()
        .OrderBy(g => g.Id).ToList();

    public Group GetGroup(int id) => _db.Groups.Find(id);

    public Group GetGroupWithMembers(int id) => _db.Groups.AsNoTracking()
        .Where(g => g.Id == id).Include(g => g.Members.OrderBy(m => m.Person.FirstName).ThenBy(m => m.Person.LastName))
        .FirstOrDefault();
}
