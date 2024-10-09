using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class SectionController : Controller
    {
        private readonly SectionService _sectionService;
        private readonly EnrollmentService _enrollmentService;

        private readonly ILogger<SectionController> _logger;

        public SectionController(SectionService sectionService, EnrollmentService enrollmentServce,
            ILogger<SectionController> logger)
        {
            _sectionService = sectionService;
            _enrollmentService = enrollmentServce;
            _logger = logger;
        }

        public IActionResult Index(int? termCode)
        {
            var terms = _sectionService.GetTerms();

            Term selectedTerm;
            if (termCode != null) selectedTerm = new Term((int)termCode);
            else if (terms.Count > 0) selectedTerm = terms[0];
            else selectedTerm = null;

            var sections = selectedTerm != null ? _sectionService.GetSections(selectedTerm.Code) : null;

            ViewBag.Terms = terms;
            ViewBag.SelectedTerm = selectedTerm;

            return View(sections);
        }

        public IActionResult Search(string searchText, int? sectionId)
        {
            var sections = _sectionService.SearchSections(searchText);
            if (sections.Count > 0)
            {
                ViewBag.Section = sectionId == null ? sections[0] : _sectionService.GetSection((int)sectionId);
                ViewBag.Enrollments = _enrollmentService.GetEnrollmentsBySection(ViewBag.Section.Id);
            }

            return View(sections);
        }

        public IActionResult View(int id)
        {
            var section = _sectionService.GetSection(id);
            if (section == null) return NotFound();

            ViewBag.Enrollments = _enrollmentService.GetEnrollmentsBySection(section.Id);
            ViewBag.Sections = _sectionService.GetSections(section.Term.Code);

            return View(section);
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var section = _sectionService.GetSection(id);
            if (section == null) return NotFound();

            _sectionService.DeleteSection(section);
            _logger.LogInformation("{user} deleted section {section}", User.GetName(), id);

            return RedirectToAction("Index", new { termCode = section.Term.Code });
        }
    }
}
