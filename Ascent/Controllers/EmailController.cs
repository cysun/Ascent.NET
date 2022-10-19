using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Ascent.Models;
using Ascent.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Controllers
{
    public class EmailController : Controller
    {
        private readonly EmailSender _emailSender;
        private readonly GroupService _groupService;
        private readonly MessageService _messageService;

        private readonly IMapper _mapper;
        private readonly ILogger<EmailController> _logger;

        public EmailController(EmailSender emailSender, GroupService groupService, MessageService messageService,
            IMapper mapper, ILogger<EmailController> logger)
        {
            _emailSender = emailSender;
            _groupService = groupService;
            _messageService = messageService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Group(int groupId)
        {
            var group = _groupService.GetGroup(groupId);
            if (group == null) return NotFound();

            ViewBag.Group = group;

            return View(new MessageInputModel
            {
                Recipient = group.Name
            });
        }

        [HttpPost]
        public IActionResult Group(int groupId, MessageInputModel input)
        {
            if (!ModelState.IsValid) return View(input);

            var group = _groupService.GetGroup(groupId);
            if (group == null) return NotFound();

            var message = _mapper.Map<Message>(input);
            message.Recipient = group.Name;
            message.Author = new Author
            {
                Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                FirstName = User.FindFirstValue(ClaimTypes.GivenName),
                LastName = User.FindFirstValue(ClaimTypes.Surname),
                Email = User.FindFirstValue(ClaimTypes.Email)
            };

            var recipients = _groupService.GetMembers(group)
                .Where(p => !string.IsNullOrWhiteSpace(p.GetPreferredEmail(group.EmailPreference)))
                .Select(p => (Name: p.FullName, Email: p.GetPreferredEmail(group.EmailPreference))).ToList();
            message.IsFailed = !_emailSender.Send(message, recipients);
            message.TimeSent = DateTime.UtcNow;

            _messageService.AddMessage(message);
            _logger.LogInformation("{user} send email to group {group}", User.Identity.Name, group.Name);

            return RedirectToAction("View", "Group", new { id = group.Id });
        }
    }
}

namespace Ascent.Models
{
    public class MessageInputModel
    {
        public string Recipient { get; set; }

        [Required, MaxLength(255)]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        [Display(Name = "Use BCC")]
        public bool UseBcc { get; set; } = true;
    }
}
