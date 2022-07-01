using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class FileController : Controller
    {
        private readonly FileService _fileService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<FileController> _logger;

        public FileController(FileService fileService, IAuthorizationService authorizationService,
            IMapper mapper, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(string searchText)
        {
            List<Models.File> files;

            if (string.IsNullOrWhiteSpace(searchText))
                files = _fileService.GetFiles();
            else
                files = _fileService.SearchFiles(searchText);

            return View(files);
        }

        [HttpPost]
        public IActionResult Upload(int? parentId, IFormFile[] uploadedFiles)
        {
            foreach (var uploadedFile in uploadedFiles)
                _fileService.UploadFile(parentId, uploadedFile);

            return Ok();
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
