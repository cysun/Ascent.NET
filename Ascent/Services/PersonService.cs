using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services;

public class PersonService
{
    private readonly AppDbContext _db;

    private readonly ILogger<PersonService> _logger;

    public PersonService(AppDbContext db, ILogger<PersonService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Person GetPerson(int id) => _db.Persons.Find(id);

    public Person GetPersonByCampusId(string campusId) => _db.Persons
        .Where(p => p.CampusId == campusId).FirstOrDefault();

    // maxResults=null for unlimited results
    public List<Person> SearchPersonsByPrefix(string prefix, int? maxResults = 100)
    {
        return _db.Persons.FromSqlRaw("select * from search_persons_by_pattern({0}, {1})",
            $"{prefix}%".ToLower(), maxResults).AsNoTracking().ToList();
    }

    public void AddPerson(Person person)
    {
        _db.Persons.Add(person);
        _db.SaveChanges();
    }

    public void SaveChanges() => _db.SaveChanges();
}
