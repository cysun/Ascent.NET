using Ascent.Services;
using ImportCSNS;
using ImportCSNS.Models;
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
    Console.WriteLine($"{aproject.AcademicYear} {aproject.Title}");
    foreach (var student in cproject.Students)
        Console.WriteLine($"\t {student.User.Cin}");
    foreach (var advisor in cproject.Advisors)
        Console.WriteLine($"\t {advisor.User.FirstName}");
    foreach (var liaison in cproject.Liaisons)
        Console.WriteLine($"\t {liaison.User.FirstName}");
    foreach (var resource in cproject.Resources)
    {
        Console.Write($"\t {resource.Resource.Name}");
        if (resource.Resource.Type == ResourceType.File)
            Console.WriteLine($"({resource.Resource.File.Name})");
        else
            Console.WriteLine();
    }
}

Console.WriteLine($"There are {csnsDb.Projects.Count()} in CSNS.");
Console.WriteLine($"There are {ascentDb.Projects.Count()} in Ascent.");
