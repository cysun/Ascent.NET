using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Program.Controllers
{
    [Area("Program")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class ResourceController : Controller
    {
        private readonly ProgramService _programService;

        private readonly ILogger<ModuleController> _logger;

        public ResourceController(ProgramService programService, ILogger<ModuleController> logger)
        {
            _programService = programService;
            _logger = logger;
        }

        public IActionResult Add(int moduleId, int contentId, string type)
        {
            var resource = new ProgramResource();
            switch (type?.ToLower())
            {
                case "file":
                    resource.Type = ResourceType.File;
                    resource.FileId = contentId;
                    break;
                case "page":
                    resource.Type = ResourceType.Page;
                    resource.PageId = contentId;
                    break;
                default:
                    _logger.LogInformation("Unrecognized resource type: {type}", type);
                    return BadRequest();
            }

            _programService.AddResourceToModule(moduleId, resource);
            _logger.LogInformation("{user} added resource {resource} to module {module}", User.GetName(), resource.Id, moduleId);

            return RedirectToAction("Edit", "Module", new { id = moduleId });
        }

        public IActionResult MoveUp(int id)
        {
            var resource = _programService.GetResource(id);
            if (resource == null) return NotFound();

            _programService.MoveUpResource(resource);
            _logger.LogInformation("{user} moved up resource {resource}", User.GetName(), id);

            return RedirectToAction("Edit", "Module", new { id = resource.ModuleId });
        }

        public IActionResult MoveDown(int id)
        {
            var resource = _programService.GetResource(id);
            if (resource == null) return NotFound();

            _programService.MoveDownResource(resource);
            _logger.LogInformation("{user} moved down resource {resource}", User.GetName(), id);

            return RedirectToAction("Edit", "Module", new { id = resource.ModuleId });
        }

        public IActionResult Delete(int id)
        {
            var resource = _programService.GetResource(id);
            if (resource == null) return NotFound();

            _programService.DeleteResource(resource);
            _logger.LogInformation("{user} deleted resource {resource}", User.GetName(), id);

            return RedirectToAction("Edit", "Module", new { id = resource.ModuleId });
        }
    }
}
