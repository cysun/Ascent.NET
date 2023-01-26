using System.ComponentModel.DataAnnotations;
using Ascent.Models;
using Ascent.Security;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class GroupController : Controller
    {
        private readonly GroupService _groupService;

        private readonly IMapper _mapper;
        private readonly ILogger<GroupController> _logger;

        public GroupController(GroupService groupService, IMapper mapper, ILogger<GroupController> logger)
        {
            _groupService = groupService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_groupService.GetGroups());
        }

        public IActionResult View(int id)
        {
            var group = _groupService.GetGroup(id);
            if (group == null) return NotFound();
            if (group.IsVirtual) return RedirectToAction("Index");

            ViewBag.Members = _groupService.GetMembers(group);

            return View(group);
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add()
        {
            return View(new GroupInputModel());
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Add(GroupInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var group = _mapper.Map<Group>(input);
            _groupService.AddGroup(group);
            _logger.LogInformation("{user} added group {group}", User.Identity.Name, group.Name);

            return RedirectToAction("View", new { id = group.Id });
        }

        [HttpGet]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id)
        {
            var group = _groupService.GetGroup(id);
            if (group == null) return NotFound();
            if (group.IsVirtual) return RedirectToAction("Index");

            ViewBag.Group = group;
            ViewBag.Members = _groupService.GetMembers(group);

            return View(_mapper.Map<GroupInputModel>(group));
        }

        [HttpPost]
        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Edit(int id, GroupInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var group = _groupService.GetGroup(id);
            if (group == null) return NotFound();
            if (group.IsVirtual) return RedirectToAction("Index");

            _mapper.Map(input, group);
            _groupService.SaveChanges();
            _logger.LogInformation("{user} edited group {group}", User.Identity.Name, group.Name);

            return RedirectToAction("View", new { id = group.Id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult AddMember(int id, int personId)
        {
            _groupService.AddMemberToGroup(id, personId);
            _logger.LogInformation("{user} added person {person} to group {group}", User.Identity.Name, personId, id);

            return RedirectToAction("Edit", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult RemoveMember(int id, int personId)
        {
            _groupService.RemoveMemberFromGroup(id, personId);
            _logger.LogInformation("{user} removed person {person} from group {group}", User.Identity.Name, personId, id);

            return RedirectToAction("Edit", new { id });
        }

        [Authorize(Policy = Constants.Policy.CanWrite)]
        public IActionResult Delete(int id)
        {
            var group = _groupService.GetGroup(id);
            if (group == null) return NotFound();

            if (!group.IsVirtual)
            {
                _groupService.DeleteGroup(group);
                _logger.LogInformation("{user} deleted group {group}", User.Identity.Name, group.Name);
            }

            return RedirectToAction("Index");
        }
    }
}

namespace Ascent.Models
{
    public class GroupInputModel
    {
        [Required, MaxLength(32)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Display(Name = "Email Preference")]
        public EmailPreference EmailPreference { get; set; }
    }
}
