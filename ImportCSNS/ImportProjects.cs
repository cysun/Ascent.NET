using Ascent.Services;
using ImportCSNS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ImportCSNS;

public partial class Importer
{
    public void ImportProjects()
    {
        var inputDir = _config.GetValue<string>("InputDir");
        var outputDir = _config.GetValue<string>("OutputDir");

        // Connect to two databases
        // See https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/ on how to create DbContext

        using var csnsDb = new CsnsDbContext(_config.GetConnectionString("CsnsConnection"));

        var appDbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_config.GetConnectionString("DefaultConnection"))
            .Options;
        using var ascentDb = new AppDbContext(appDbContextOptions);

        // Get all CSNS projects

        var cprojects = csnsDb.Projects
            .Include(p => p.Students).ThenInclude(s => s.User)
            .Include(p => p.Advisors).ThenInclude(a => a.User)
            .Include(p => p.Liaisons).ThenInclude(l => l.User)
            .Include(p => p.Resources).ThenInclude(p => p.Resource).ThenInclude(r => r.File)
            .ToList();

        // Import each CSNS project to Ascent
        int count = 0;
        foreach (var cproject in csnsDb.Projects)
        {
            // if (count >= 1) break;

            var aproject = new Ascent.Models.Project()
            {
                AcademicYear = $"{cproject.Year - 1}-{cproject.Year}",
                Title = cproject.Title,
                Description = cproject.Description,
                Sponsor = cproject.Sponsor,
            };

            foreach (var cstudent in cproject.Students)
            {
                var person = ascentDb.Persons.Where(p => p.CampusId == cstudent.User.Cin).FirstOrDefault();
                if (person == null)
                {
                    Console.WriteLine($"ERROR: Cannot find student with CIN={cstudent.User.Cin}");
                    continue;
                }
                aproject.Members.Add(new Ascent.Models.ProjectMember()
                {
                    Project = aproject,
                    Person = person,
                    Type = Ascent.Models.Project.MemberType.Student
                });
            }

            foreach (var cadvisor in cproject.Advisors)
            {
                var person = ascentDb.Persons.Where(p => p.CampusId == cadvisor.User.Cin).FirstOrDefault();
                if (person == null)
                {
                    Console.WriteLine($"ERROR: Cannot find advisor with CIN={cadvisor.User.Cin}");
                    continue;
                }
                aproject.Members.Add(new Ascent.Models.ProjectMember()
                {
                    Project = aproject,
                    Person = person,
                    Type = Ascent.Models.Project.MemberType.Advisor
                });
            }

            foreach (var cliaison in cproject.Liaisons)
            {
                var person = ascentDb.Persons.Where(p => p.CampusId == cliaison.User.Cin).FirstOrDefault();
                if (person == null)
                {
                    Console.WriteLine($"ERROR: Cannot find liaison with CIN={cliaison.User.Cin}");
                    continue;
                }
                aproject.Members.Add(new Ascent.Models.ProjectMember()
                {
                    Project = aproject,
                    Person = person,
                    Type = Ascent.Models.Project.MemberType.Liaison
                });
            }

            ascentDb.Projects.Add(aproject);
            ascentDb.SaveChanges();

            // Add Resources
            foreach (var cresource in cproject.Resources)
            {
                if (cresource.Resource.Type == ResourceType.None) continue;

                var item = new Ascent.Models.ProjectResource()
                {
                    Project = aproject,
                    Name = cresource.Resource.Name,
                    IsPrivate = cresource.Resource.Private
                };
                aproject.Resources.Add(item);

                switch (cresource.Resource.Type)
                {
                    case ResourceType.Text:
                        item.Type = Ascent.Models.ResourceType.Text;
                        item.Text = cresource.Resource.Text;
                        break;
                    case ResourceType.Url:
                        item.Type = Ascent.Models.ResourceType.Url;
                        item.Url = cresource.Resource.Url;
                        break;
                    case ResourceType.File:
                        var cfile = csnsDb.Files.Find(cresource.Resource.FileId);
                        if (cfile == null)
                        {
                            Console.WriteLine($"ERROR: Cannot find file with id={cresource.Resource.FileId}");
                            break;
                        }
                        var physicalFile = Path.Combine(inputDir, cfile.Id.ToString());
                        if (!System.IO.File.Exists(physicalFile))
                        {
                            Console.WriteLine($"ERROR: Cannot find physical file with id={cresource.Resource.FileId}");
                            break;
                        }
                        item.Type = Ascent.Models.ResourceType.File;
                        var afile = new Ascent.Models.File()
                        {
                            Name = cfile.Name,
                            ContentType = cfile.Type,
                            Size = (long)cfile.Size,
                            TimeCreated = ((DateTime)cfile.Date).ToUniversalTime()
                        };
                        item.File = afile;
                        ascentDb.SaveChanges();

                        var outFile = Path.Combine(outputDir, $"{afile.Id}-{afile.Version}");
                        System.IO.File.Copy(physicalFile, outFile);
                        break;

                    default:
                        Console.WriteLine($"Unknown resource type: ${cresource.Resource.Type}");
                        break;
                }
            }

            Console.WriteLine($"Imported project {aproject.Title}");
            ++count;
        }
    }
}
