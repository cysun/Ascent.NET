using Ascent.Helpers;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Import.Controllers
{
    [Area("Import")]
    [Authorize(Policy = Constants.Policy.CanWrite)]
    public class CanvasController : Controller
    {
        private readonly PersonService _personService;
        private readonly ILogger<CanvasController> _logger;

        enum ImportResultType
        {
            Created = 0,
            Updated = 1,
            Unchanged = 2
        };

        public CanvasController(PersonService personService, ILogger<CanvasController> logger)
        {
            _personService = personService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult ImportStudents()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ImportStudents(IFormFile uploadedFile)
        {
            int[] results = new int[3];
            var excelReader = new ExcelReader(uploadedFile.OpenReadStream());
            while (excelReader.Next())
                results[(int)CreateOrUpdatePerson(excelReader)]++;

            return View("ImportStudentsResult", results);
        }

        private ImportResultType CreateOrUpdatePerson(ExcelReader excelReader)
        {
            var result = ImportResultType.Unchanged;
            var cin = excelReader.Get("SIS User ID");
            var person = _personService.GetPersonByCampusId(cin);
            if (person == null)
            {
                result = ImportResultType.Created;
                var (firstName, lastName) = Utils.SplitName(excelReader.Get("Student"));
                person = new Person
                {
                    CampusId = cin,
                    FirstName = firstName,
                    LastName = lastName,
                    SchoolEmail = excelReader.Get("SIS Login ID"),
                    CanvasId = int.Parse(excelReader.Get("ID"))
                };
                _personService.AddPerson(person);
                _logger.LogInformation("New person [{cin}, {firstName} {lastName}] created during import.", cin, firstName, lastName);
            }
            else
            {
                if (person.CanvasId == null)
                {
                    person.CanvasId = int.Parse(excelReader.Get("ID"));
                    result = ImportResultType.Updated;
                }
                if (string.IsNullOrEmpty(person.SchoolEmail))
                {
                    person.SchoolEmail = excelReader.Get("SIS Login ID");
                    result = ImportResultType.Updated;
                }
                _personService.SaveChanges();
            }

            return result;
        }
    }
}
