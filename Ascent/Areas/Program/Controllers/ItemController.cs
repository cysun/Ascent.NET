using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Program.Controllers
{
    [Area("Program")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class ItemController : Controller
    {
        private readonly ProgramService _programService;

        private readonly ILogger<ModuleController> _logger;

        public ItemController(ProgramService programService, ILogger<ModuleController> logger)
        {
            _programService = programService;
            _logger = logger;
        }

        public IActionResult Add(int moduleId, int contentId, string type)
        {
            var item = new ProgramItem();
            switch (type?.ToLower())
            {
                case "file":
                    item.Type = ItemType.File;
                    item.FileId = contentId;
                    break;
                case "page":
                    item.Type = ItemType.Page;
                    item.PageId = contentId;
                    break;
                default:
                    _logger.LogInformation("Unrecognized item type: {type}", type);
                    return BadRequest();
            }

            _programService.AddItemToModule(moduleId, item);
            _logger.LogInformation("{user} added item {item} to module {module}", User.Identity.Name, item.Id, moduleId);

            return RedirectToAction("Edit", "Module", new { id = moduleId });
        }

        public IActionResult MoveUp(int id)
        {
            var item = _programService.GetItem(id);
            if (item == null) return NotFound();

            _programService.MoveUpItem(item);
            _logger.LogInformation("{user} moved up item {item}", User.Identity.Name, id);

            return RedirectToAction("Edit", "Module", new { id = item.ModuleId });
        }

        public IActionResult MoveDown(int id)
        {
            var item = _programService.GetItem(id);
            if (item == null) return NotFound();

            _programService.MoveDownItem(item);
            _logger.LogInformation("{user} moved down item {item}", User.Identity.Name, id);

            return RedirectToAction("Edit", "Module", new { id = item.ModuleId });
        }

        public IActionResult Delete(int id)
        {
            var item = _programService.GetItem(id);
            if (item == null) return NotFound();

            _programService.DeleteItem(item);
            _logger.LogInformation("{user} deleted item {item}", User.Identity.Name, id);

            return RedirectToAction("Edit", "Module", new { id = item.ModuleId });
        }
    }
}
