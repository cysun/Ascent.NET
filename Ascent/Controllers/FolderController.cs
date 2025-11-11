using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class FolderController : Controller
    {
        private readonly FileService _fileService;

        private readonly IAuthorizationService _authorizationService;

        public FolderController(FileService fileService, IAuthorizationService authorizationService)
        {
            _fileService = fileService;
            _authorizationService = authorizationService;
        }

        [AllowAnonymous]
        public async Task<IActionResult> ViewAsync(int id)
        {
            var folder = _fileService.GetFolder(id);
            if (folder == null) return NotFound();

            var canRead = (await _authorizationService.AuthorizeAsync(User, Constants.Policy.CanRead)).Succeeded;
            if (!folder.IsPublic && !canRead)
                return Forbid();

            if (canRead)
                ViewBag.Ancestors = _fileService.GetAncestors(folder);

            return View(folder);
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Create(string name, int? parentId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest();

            var folder = new Models.File
            {
                Name = name,
                ParentId = parentId,
                IsFolder = true,
                IsRegular = true
            };
            _fileService.AddFolder(folder);
            _fileService.SaveChanges();

            return Ok();
        }

        public List<Models.File> GetChildFolders(int? parentId)
        {
            return _fileService.GetChildren(parentId, true);
        }
    }
}
