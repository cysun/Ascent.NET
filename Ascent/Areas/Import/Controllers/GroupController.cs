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
    public class GroupController : Controller
    {
        private readonly PersonService _personService;
        private readonly GroupService _groupService;

        public GroupController(PersonService personService, GroupService groupService)
        {
            _personService = personService;
            _groupService = groupService;
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(_groupService.GetRegularGroups());
        }

        [HttpPost]
        public IActionResult Import(int groupId, IFormFile uploadedFile)
        {
            var excelReader = new ExcelReader(uploadedFile.OpenReadStream());
            while (excelReader.Next())
            {
                var person = GetOrCreatePerson(excelReader);
                _groupService.AddMemberToGroup(groupId, person.Id);
            }
            return RedirectToAction("View", "Group", new { Area = "", id = groupId });
        }

        private Person GetOrCreatePerson(ExcelReader excelReader)
        {
            var cin = excelReader.Get("CIN");
            var person = _personService.GetPersonByCampusId(cin);
            if (person == null)
            {
                person = new Person
                {
                    CampusId = cin,
                    FirstName = excelReader.Get("FirstName"),
                    LastName = excelReader.Get("LastName")
                };
                _personService.AddPerson(person);
            }

            if (excelReader.HasColumn("Email"))
            {
                // There could be multiple emails 
                var emails = excelReader.Get("Email").Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var email in emails)
                    person.UpdateEmail(email);
                _personService.SaveChanges();
            }

            return person;
        }
    }
}
