using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.XSSF.UserModel;

namespace Ascent.Areas.Project.Controllers
{
    [Area("Project")]
    [Authorize] // The default policy is CanRead; here we only need Authenticated for the operations that check CanManageProject.
    public class ProjectController : Controller
    {
        private readonly PersonService _personService;
        private readonly ProjectService _projectService;

        private readonly IAuthorizationService _authorizationService;

        private readonly AppMapper _mapper;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(PersonService personService, ProjectService projectService,
            IAuthorizationService authorizationService, AppMapper mapper, ILogger<ProjectController> logger)
        {
            _personService = personService;
            _projectService = projectService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Index(string year)
        {
            var academicYears = _projectService.GetAcademicYears();
            var academicYear = year != null ? year : academicYears.FirstOrDefault();

            ViewBag.AcademicYears = academicYears;
            ViewBag.AcademicYear = academicYear;

            return View(_projectService.GetProjects(academicYear));
        }

        [AllowAnonymous]
        public IActionResult View(int id)
        {
            var project = _projectService.GetFullProject(id);
            if (project == null) return NotFound();

            return View(project);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            var currentYear = DateTime.Now.Year;
            return View(new ProjectInputModel()
            {
                AcademicYear = $"{currentYear}-{currentYear + 1}"
            });
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(ProjectInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var project = _mapper.Map(input);
            _projectService.AddProject(project);
            _logger.LogInformation("{user} created project {project}", User.GetName(), project.Id);

            return RedirectToAction("Members", new { id = project.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, id, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            var project = _projectService.GetProject(id);
            if (project == null) return NotFound();

            ViewBag.Project = project;

            return View(_mapper.Map(project));
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(int id, ProjectInputModel input)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, id, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            if (!ModelState.IsValid) return View(input);

            var project = _projectService.GetProject(id);
            if (project == null) return NotFound();

            _mapper.Map(input, project);
            _projectService.SaveChanges();
            _logger.LogInformation("{user} edited project {project}", User.GetName(), id);

            return RedirectToAction("View", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var project = _projectService.GetProject(id);
            if (project == null) return NotFound();

            project.IsDeleted = true;
            _projectService.SaveChanges();
            _logger.LogInformation("{user} deleted project {project}", User.GetName(), id);

            return RedirectToAction("Index", new { year = project.AcademicYear });
        }

        [AllowAnonymous]
        public IActionResult Search(string searchText, int? sectionId)
        {
            return View(_projectService.SearchProjects(searchText));
        }

        [AllowAnonymous]
        public IActionResult ByMember(int memberId, string memberType)
        {
            var member = _personService.GetPerson(memberId);
            if (member == null) return NotFound();

            var ok = Enum.TryParse(memberType, out Models.Project.MemberType type);
            if (!ok) return BadRequest();

            ViewBag.Member = member;
            ViewBag.MemberType = type;

            return View(_projectService.GetProjectsByMember(memberId, type));
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Members(int id)
        {
            var project = _projectService.GetProjectWithMembers(id);
            if (project == null) return NotFound();

            return View(project);
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult AddMember(int id, int personId, string memberType)
        {
            _projectService.AddProjectMember(id, personId, memberType);
            _logger.LogInformation("{user} added member {member} to project {project}", User.GetName(), personId, id);

            return RedirectToAction("Members", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult RemoveMember(int id, int personId, string memberType)
        {
            _projectService.RemoveProjectMember(id, personId, memberType);
            _logger.LogInformation("{user} removed member {member} from project {project}", User.GetName(), personId, id);

            return RedirectToAction("Members", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Excel(string year)
        {
            var academicYear = year ?? _projectService.GetAcademicYear();
            var projects = _projectService.GetProjects(academicYear);

            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Projects");

            var headerRow = sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Year");
            headerRow.CreateCell(1).SetCellValue("Title");
            headerRow.CreateCell(2).SetCellValue("Sponsor");
            headerRow.CreateCell(3).SetCellValue("Advisors");
            headerRow.CreateCell(4).SetCellValue("Liaisons");
            headerRow.CreateCell(5).SetCellValue("Students");
            headerRow.CreateCell(6).SetCellValue("Description");

            for (int i = 0; i < projects.Count; i++)
            {
                var project = projects[i];
                var row = sheet.CreateRow(i + 1);
                row.CreateCell(0).SetCellValue(project.AcademicYear);
                row.CreateCell(1).SetCellValue(project.Title);
                row.CreateCell(2).SetCellValue(project.Sponsor);
                row.CreateCell(3).SetCellValue(string.Join(", ", project.Advisors.Select(a => a.Person.FullName)));
                row.CreateCell(4).SetCellValue(string.Join(", ", project.Liaisons.Select(l => l.Person.FullName)));
                row.CreateCell(5).SetCellValue(string.Join(", ", project.Students.Select(s => s.Person.FullName)));
            }

            using var stream = new MemoryStream();
            workbook.Write(stream);

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Projects {academicYear}.xlsx");
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Word(string year)
        {
            var academicYear = year ?? _projectService.GetAcademicYear();
            var projects = _projectService.GetProjects(academicYear);

            using var stream = new MemoryStream();
            using (var wordDoc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                WordUtils.AddStyles(mainPart);
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body!;

                WordUtils.AddHeading1(body, $"Computer Science Senior Design Projects {academicYear}");

                foreach (var project in projects)
                {
                    WordUtils.AddLinkedText(mainPart, project.Title,
                        $"https://ascent.cysun.org/project/project/view/{project.Id}");

                    if (!string.IsNullOrEmpty(project.Sponsor))
                        WordUtils.AddParagraph(body, $"Sponsor: {project.Sponsor}");

                    if (project.Advisors.Count > 0)
                        WordUtils.AddParagraph(body,
                            $"Advisor(s): {string.Join(", ", project.Advisors.Select(a => a.Person.FullName))}");

                    if (project.Liaisons.Count > 0)
                        WordUtils.AddParagraph(body,
                            $"Liaison(s): {string.Join(", ", project.Liaisons.Select(l => l.Person.FullName))}");

                    if (project.Students.Count > 0)
                        WordUtils.AddParagraph(body,
                            $"Students: {string.Join(", ", project.Students.Select(s => s.Person.FullName))}");

                    WordUtils.AddParagraph(body, $"Description:");
                    body.Append(new Paragraph());
                }

                mainPart.Document.Save();
            }

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"Projects {academicYear}.docx");
        }
    }
}

namespace Ascent.Models
{
    public class ProjectInputModel
    {
        // In the form of xxxx-yyyy, e.g. 2022-2023
        [MaxLength(12), Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string Sponsor { get; set; }
    }
}
