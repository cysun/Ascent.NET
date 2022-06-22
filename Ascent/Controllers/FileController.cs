using Ascent.Models;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class FileController : Controller
    {
        private readonly FileService _fileService;

        private readonly IAuthorizationService _authorizationService;

        public FileController(FileService fileService, IAuthorizationService authorizationService)
        {
            _fileService = fileService;
            _authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> ViewAsync(int id)
        {
            return await DownloadAsync(id, true);
        }

        [AllowAnonymous]
        public async Task<IActionResult> DownloadAsync(int id, bool inline = false)
        {
            var file = _fileService.GetFile(id);
            if (file == null) return NotFound();
            if (file.IsFolder) return BadRequest();

            if (!file.IsPublic && !(await _authorizationService.AuthorizeAsync(User, Constants.Policy.CanRead)).Succeeded)
                return Forbid();

            var diskFile = _fileService.GetDiskFile(file.Id, file.Version);

            inline = inline && !_fileService.IsAttachmentType(file.Name);
            return !inline ? PhysicalFile(diskFile, file.ContentType, file.Name) :
                PhysicalFile(diskFile, _fileService.IsTextType(file.Name) ? "text/plain" : file.ContentType);
        }
    }
}
