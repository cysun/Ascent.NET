using System.ComponentModel.DataAnnotations;
using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Project.Controllers
{
    [Area("Project")]
    [Authorize] // The default policy is CanRead; here we only need Authenticated for the operations that check CanManageProject.
    public class ResourceController : Controller
    {
        private readonly FileService _fileService;
        private readonly ProjectService _projectService;

        private readonly IAuthorizationService _authorizationService;

        private readonly AppMapper _mapper;
        private readonly ILogger<ResourceController> _logger;

        public ResourceController(FileService fileService, ProjectService projectService,
            IAuthorizationService authorizationService, AppMapper mapper, ILogger<ResourceController> logger)
        {
            _fileService = fileService;
            _projectService = projectService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync(int projectId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, projectId, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            var project = _projectService.GetProjectWithResources(projectId);
            if (project == null) return NotFound();

            return View(project);
        }

        [HttpGet]
        public async Task<IActionResult> AddAsync(int projectId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, projectId, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            var project = _projectService.GetProject(projectId);
            if (project == null) return NotFound();

            ViewBag.Project = project;

            return View(new ProjectResourceInputModel()
            {
                ProjectId = projectId
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddAsync(ProjectResourceInputModel input, IFormFile uploadedFile)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, input.ProjectId, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            var resource = _mapper.Map(input);
            if (resource.Type == ResourceType.File && uploadedFile != null)
            {
                var file = await _fileService.UploadFileAsync(null, uploadedFile, false);
                file.IsPublic = !resource.IsPrivate;
                resource.FileId = file.Id;
            }

            _projectService.AddProjectResource(resource);
            _logger.LogInformation("{user} added resource {resource} to project {project}", User.GetName(),
                resource.Id, resource.ProjectId);

            return RedirectToAction("Index", new { projectId = resource.ProjectId });
        }

        [HttpGet]
        public async Task<IActionResult> EditAsync(int id)
        {
            var resource = _projectService.GetProjectResource(id);
            if (resource == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource.ProjectId, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            ViewBag.Resource = resource;

            return View(_mapper.Map(resource));
        }

        public async Task<IActionResult> DeleteAsync(int id)
        {
            var resource = _projectService.GetProjectResource(id);
            if (resource == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, resource.ProjectId, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            _projectService.RemoveProjectResource(resource);
            _logger.LogInformation("{user} removed resource {resource} from project {project}", User.GetName(),
                resource.Id, resource.ProjectId);

            return RedirectToAction("Index", new { projectId = resource.ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(int id, ProjectResourceInputModel input, IFormFile uploadedFile)
        {
            var resource = _projectService.GetProjectResource(id);
            if (resource == null) return NotFound();

            var authResult = await _authorizationService.AuthorizeAsync(User, input.ProjectId, Constants.Policy.CanManageProject);
            if (!authResult.Succeeded)
                return Forbid();

            _mapper.Map(input, resource);
            if (resource.Type == ResourceType.File && uploadedFile != null)
            {
                var file = await _fileService.UploadFileAsync(null, uploadedFile, false);
                file.IsPublic = !resource.IsPrivate;
                resource.FileId = file.Id;
            }

            _projectService.SaveChanges();
            _logger.LogInformation("{user} edited resource {resource} of project {project}", User.GetName(),
                resource.Id, resource.ProjectId);

            return RedirectToAction("Index", new { projectId = resource.ProjectId });
        }

        [AllowAnonymous]
        public async Task<IActionResult> GetAsync(int id, bool inline = false)
        {
            var resource = _projectService.GetProjectResource(id);
            if (resource == null) return NotFound();

            if (resource.IsPrivate &&
                !(await _authorizationService.AuthorizeAsync(User, resource.ProjectId, Constants.Policy.CanManageProject)).Succeeded)
                return Forbid();

            switch (resource.Type)
            {
                case ResourceType.Url:
                    return Redirect(resource.Url);
                case ResourceType.File:
                    return await DownloadFileAsync((int)resource.FileId, inline);
                default:
                    return BadRequest();
            }
        }

        private async Task<IActionResult> DownloadFileAsync(int id, bool inline = false)
        {
            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();
            if (file.IsFolder) return BadRequest();

            return Redirect(await _fileService.GetDownloadUrlAsync(file, inline));
        }
    }
}

namespace Ascent.Models
{
    public class ProjectResourceInputModel
    {
        public int ProjectId { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        public ResourceType Type { get; set; }

        public string Text { get; set; }

        [MaxLength(2000)]
        public string Url { get; set; }

        [Display(Name = "Private")]
        public bool IsPrivate { get; set; }
    }
}
