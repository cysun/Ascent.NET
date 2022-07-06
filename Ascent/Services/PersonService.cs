using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class PersonService
{
    private readonly AppDbContext _db;

    public PersonService(AppDbContext db) { _db = db; }

    public Person GetPerson(int id) => _db.Persons.Find(id);

    public Person GetPersonByCampusId(string campusId) => _db.Persons
        .Where(p => p.CampusId == campusId).FirstOrDefault();

    // maxResults=null for unlimited results
    public List<Person> SearchPersonsByPrefix(string prefix, int? maxResults = 100)
    {
        if (prefix == null || prefix.Length < 2) return new List<Person>();

        return _db.Persons.FromSqlRaw("SELECT * FROM \"SearchPersons\"({0}, {1})",
            $"{prefix}%".ToLower(), maxResults).AsNoTracking().ToList();
    }

    public void AddPerson(Person person)
    {
        _db.Persons.Add(person);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
