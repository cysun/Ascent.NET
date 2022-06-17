using Ascent.Models;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Services
{
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

        public List<Person> SearchPersonsByPrefix(string prefix, int? maxResults = null)
        {
            return _db.Persons.FromSqlRaw("select * from search_persons_by_pattern({0}, {1})",
                $"{prefix}%".ToLower(), maxResults).AsNoTracking().ToList();
        }

        public void SaveChanges() => _db.SaveChanges();
    }
}
