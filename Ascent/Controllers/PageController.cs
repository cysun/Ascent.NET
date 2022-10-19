using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class PageController : Controller
    {
        private readonly PageService _pageService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IMapper _mapper;
        private readonly ILogger<PageController> _logger;

        public PageController(PageService pageService, IAuthorizationService authorizationService,
            IMapper mapper, ILogger<PageController> logger)
        {
            _pageService = pageService;
            _authorizationService = authorizationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.PinnedPages = _pageService.GetPinnedPages();
            ViewBag.LastViewedPages = _pageService.GetLastViwedPages();

            return View();
        }

        public IActionResult Search(string searchText)
        {
            var searchResults = string.IsNullOrWhiteSpace(searchText) ? new List<Page>() : _pageService.SearchPages(searchText);

            return View(searchResults);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ViewAsync(int id)
        {
            var page = _pageService.GetPage(id);
            if (page == null) return NotFound();

            if (!page.IsPublic && !(await _authorizationService.AuthorizeAsync(User, Constants.Policy.CanRead)).Succeeded)
                return Forbid();

            page.TimeViewed = DateTime.UtcNow;
            _pageService.SaveChanges();

            return View(page);
        }


        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Create()
        {
            return View(new PageInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Create(PageInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var page = _mapper.Map<Page>(input);
            page.IsRegular = true; // pages created via Page UI are "regular"
            _pageService.AddPage(page);
            _logger.LogInformation("{user} created page {page}", User.Identity.Name, page.Id);

            return RedirectToAction("Edit", new { id = page.Id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var page = _pageService.GetPage(id);
            if (page == null) return NotFound();

            return View(page);
        }

        [HttpPut("page/{id}/{field}")]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult SetField(int id, [Required] string field, string value)
        {
            var page = _pageService.GetPage(id);
            if (page == null) return NotFound();

            switch (field.ToLower())
            {
                case "subject":
                    page.Subject = value;
                    break;
                case "content":
                    page.Content = value;
                    break;
                case "public":
                    if (page.IsRegular) page.IsPublic = !page.IsPublic;
                    break;
                case "pinned":
                    if (page.IsRegular) page.IsPinned = !page.IsPinned;
                    break;
                default:
                    _logger.LogWarning("Unrecognized field: {field}", field);
                    return BadRequest();
            }

            page.TimeUpdated = DateTime.UtcNow;
            page.TimeViewed = DateTime.UtcNow;
            _pageService.SaveChanges();
            _logger.LogInformation("{user} set page {page} field {field} to {value}", User.Identity.Name,
                page.Id, field, value);

            return Ok();
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var page = _pageService.GetPage(id);
            if (page == null) return NotFound();

            _pageService.DeletePage(page);
            _logger.LogInformation("{user} deleted page {page}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }

        public IActionResult Revisions(int id, int? version)
        {
            var page = _pageService.GetPage(id);
            if (page == null) return NotFound();

            ViewBag.Page = page;
            ViewBag.Version = version ?? page.Version;
            ViewBag.Revisions = _pageService.GetPageRevisions(id);

            return View(version == null || version == page.Version ? _mapper.Map<PageRevision>(page)
                : _pageService.GetPageRevision(id, (int)version));
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult SaveRevision(int id)
        {
            var page = _pageService.GetPage(id);
            if (page == null) return NotFound();

            var revision = _mapper.Map<PageRevision>(page);
            page.Version++;
            _pageService.AddPageRevision(revision);
            _logger.LogInformation("{user} created revision {version} of page {page}",
                User.Identity.Name, revision.Version, id);

            return RedirectToAction("Revisions", new { id = id, version = revision.Version });
        }

        public IActionResult RevertToRevision(int id, int version)
        {
            var revision = _pageService.GetPageRevision(id, version);
            if (revision == null) return NotFound();

            var page = _pageService.GetPage(id);
            page.Subject = revision.Subject;
            page.Content = revision.Content;
            page.TimeUpdated = DateTime.UtcNow;
            page.TimeViewed = DateTime.UtcNow;

            _pageService.SaveChanges();
            _logger.LogInformation("{user} reverted page {page} to revision {version}", User.Identity.Name, id, version);

            return RedirectToAction("Revisions", new { id = id });
        }

        public List<Page> Autocomplete(string searchText)
        {
            return _pageService.SearchPages(searchText, 10);
        }
    }
}

namespace Ascent.Models
{
    public class PageInputModel
    {
        [Required]
        [MaxLength(80)]
        public string Subject { get; set; }

        public string Content { get; set; }

        public bool IsPinned { get; set; }
        public bool IsPublic { get; set; }
    }
}
