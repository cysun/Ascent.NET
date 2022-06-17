using Ascent.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class PersonController : Controller
    {
        private readonly PersonService _personService;

        private readonly ILogger<PersonController> _logger;

        public PersonController(PersonService personService, ILogger<PersonController> logger)
        {
            _personService = personService;
            _logger = logger;
        }

        public IActionResult Index(string searchText)
        {
            return View(string.IsNullOrEmpty(searchText) ? null : _personService.SearchPersonsByPrefix(searchText));
        }
    }
}
