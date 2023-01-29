using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ProjectService _projectService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(ProjectService projectService, IAuthorizationService authorizationService,
            IMapper mapper, ILogger<ProjectController> logger)
        {
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

            var project = _mapper.Map<Project>(input);
            _projectService.AddProject(project);
            _logger.LogInformation("{user} created project {project}", User.Identity.Name, project.Id);

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

            return View(_mapper.Map<ProjectInputModel>(project));
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
            _logger.LogInformation("{user} edited project {project}", User.Identity.Name, id);

            return RedirectToAction("View", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var project = _projectService.GetProject(id);
            if (project == null) return NotFound();

            project.IsDeleted = true;
            _projectService.SaveChanges();
            _logger.LogInformation("{user} deleted project {project}", User.Identity.Name, id);

            return RedirectToAction("Index", new { year = project.AcademicYear });
        }

        public async Task<IActionResult> MembersAsync(int id)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, id, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            var project = _projectService.GetFullProject(id);
            if (project == null) return NotFound();

            return View(project);
        }

        public async Task<IActionResult> AddMemberAsync(int id, int personId, string memberType)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, id, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            _projectService.AddProjectMember(id, personId, memberType);
            _logger.LogInformation("{user} added member {member} to project {project}", User.Identity.Name, personId, id);

            return RedirectToAction("Members", new { id });
        }

        public async Task<IActionResult> RemoveMemberAsync(int id, int personId, string memberType)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, id, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            _projectService.RemoveProjectMember(id, personId, memberType);
            _logger.LogInformation("{user} removed member {member} from project {project}", User.Identity.Name, personId, id);

            return RedirectToAction("Members", new { id });
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
