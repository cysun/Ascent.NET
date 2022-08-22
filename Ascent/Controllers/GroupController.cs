using Ascent.Services;
using AutoMapper;
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
    }
}
