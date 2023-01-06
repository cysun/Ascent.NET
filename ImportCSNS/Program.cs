using Ascent.Services;
using ImportCSNS;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("csnssettings.json")
    .Build();

// See https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ on how to create DbContext

using var csnsDb = new CsnsDbContext(config.GetConnectionString("CsnsConnection"));

var appDbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(config.GetConnectionString("DefaultConnection"))
    .Options;
using var ascentDb = new AppDbContext(appDbContextOptions);

var cprojects = csnsDb.Projects
    .Include(p => p.Students).ThenInclude(s => s.User)
    .Include(p => p.Advisors).ThenInclude(a => a.User)
    .Include(p => p.Liaisons).ThenInclude(l => l.User)
    .Include(p => p.Resources).ThenInclude(p => p.Resource).ThenInclude(r => r.File)
    .ToList();

foreach (var cproject in csnsDb.Projects)
{
    var aproject = new Ascent.Models.Project()
    {
        AcademicYear = $"{cproject.Year - 1}-{cproject.Year}",
        Title = cproject.Title,
        Description = cproject.Description,
        Sponsor = cproject.Sponsor,
        IsPrivate = cproject.Private
    };

    foreach (var cstudent in cproject.Students)
    {
        var person = ascentDb.Persons.Where(p => p.CampusId == cstudent.User.Cin).FirstOrDefault();
        if (person == null)
            Console.WriteLine($"ERROR: Cannot find student with CIN={cstudent.User.Cin}");
        aproject.Students.Add(new Ascent.Models.ProjectStudent()
        {
            Project = aproject,
            Person = person
        });
    }

    foreach (var cadvisor in cproject.Advisors)
    {
        var person = ascentDb.Persons.Where(p => p.CampusId == cadvisor.User.Cin).FirstOrDefault();
        if (person == null)
            Console.WriteLine($"ERROR: Cannot find advisor with CIN={cadvisor.User.Cin}");
        aproject.Advisors.Add(new Ascent.Models.ProjectAdvisor()
        {
            Project = aproject,
            Person = person
        });
    }

    foreach (var cliaison in cproject.Liaisons)
    {
        var person = ascentDb.Persons.Where(p => p.CampusId == cliaison.User.Cin).FirstOrDefault();
        if (person == null)
            Console.WriteLine($"ERROR: Cannot find liaison with CIN={cliaison.User.Cin}");
        aproject.Liaisons.Add(new Ascent.Models.ProjectLiaison()
        {
            Project = aproject,
            Person = person
        });
    }

    ascentDb.Projects.Add(aproject);
    ascentDb.SaveChanges();
}
