using Ascent.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class ImportController : Controller
    {
        private readonly PersonService _personService;
        private readonly EnrollmentService _enrollmentService;

        private readonly ILogger<ImportController> _logger;

        public ImportController(PersonService personService, EnrollmentService enrollmentService,
            ILogger<ImportController> logger)
        {
            _personService = personService;
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
