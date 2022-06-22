using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class PersonController : Controller
    {
        private readonly PersonService _personService;

        private readonly IMapper _mapper;
        private readonly ILogger<PersonController> _logger;

        public PersonController(PersonService personService, IMapper mapper, ILogger<PersonController> logger)
        {
            _personService = personService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index(string searchText)
        {
            return View(string.IsNullOrEmpty(searchText) ? null : _personService.SearchPersonsByPrefix(searchText));
        }

        public IActionResult View(int id)
        {
            var person = _personService.GetPerson(id);
            if (person == null) return NotFound();

            return View(person);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var person = _personService.GetPerson(id);
            if (person == null) return NotFound();

            ViewBag.Person = person;

            return View(_mapper.Map<PersonInputModel>(person));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, PersonInputModel input)
        {
            var person = _personService.GetPerson(id);
            if (person == null) return NotFound();

            _mapper.Map(input, person);
            _personService.SaveChanges();
            _logger.LogInformation("{user} edited person {person}", User.Identity.Name, id);

            return RedirectToAction("View", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var person = _personService.GetPerson(id);
            if (person == null) return NotFound();

            person.IsDeleted = true;
            _personService.SaveChanges();
            _logger.LogInformation("{user} deleted person {person}", User.Identity.Name, id);

            return RedirectToAction("Index");
        }

        public List<Person> Autocomplete(string searchText)
        {
            return _personService.SearchPersonsByPrefix(searchText, 10);
        }
    }
}

namespace Ascent.Models
{
    public class PersonInputModel
    {
        [Required, MaxLength(100), Display(Name = "CIN")]
        public string CampusId { get; set; }

        [Required, MaxLength(255), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, MaxLength(255), Display(Name = "Last Name")]
        public string LastName { get; set; }

        [MaxLength(255), Display(Name = "Personal Email")]
        public string PersonalEmail { get; set; }

        [MaxLength(255), Display(Name = "School Email")]
        public string SchoolEmail { get; set; }

        [Display(Name = "BG Term")]
        public string BgTerm { get; set; }

        [Display(Name = "GG Term")]
        public string MgTerm { get; set; }

        public bool IsInstructor { get; set; }
    }
}
