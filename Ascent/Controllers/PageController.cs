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

        public IActionResult Index(string searchText)
        {
            ViewBag.LastViewedPages = _pageService.GetLastViwedPages();
            return View(!string.IsNullOrEmpty(searchText) ? _pageService.SearchPages(searchText) : null);
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
        public IActionResult Create()
        {
            return View(new PageInputModel());
        }

        [HttpPost]
        public IActionResult Create(PageInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var page = _mapper.Map<Page>(input);
            _pageService.AddPage(page);
            _logger.LogInformation("{user} created page {page}", User.Identity.Name, page.Id);

            return RedirectToAction("Edit", new { id = page.Id });
        }

        public IActionResult Edit(int id)
        {
            var page = _pageService.GetPage(id);
            if (page == null) return NotFound();

            ViewBag.Page = page;

            return View(_mapper.Map<PageInputModel>(page));
        }

        [HttpPut("page/{id}/{field}")]
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
                    page.IsPublic = !page.IsPublic;
                    break;
                default:
                    _logger.LogWarning("Unrecognized field: {field}", field);
                    break;
            }

            page.TimeUpdated = DateTime.UtcNow;
            page.TimeViewed = DateTime.UtcNow;
            _pageService.SaveChanges();

            return Ok();
        }

        public IActionResult Delete(int id)
        {
            var page = _pageService.GetPage(id);
            if (page == null) return NotFound();

            _pageService.DeletePage(page);
            _logger.LogInformation("{user} deleted page {page}", User.Identity.Name, id);

            return RedirectToAction("Index");
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

        public bool IsPublic { get; set; }
    }
}
