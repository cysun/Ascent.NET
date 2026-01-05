using Ascent.Areas.Canvas.Models;
using Ascent.Areas.Canvas.Services;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    [Authorize(Policy = Constants.Policy.HasCat)]
    public class CourseController : Controller
    {
        private readonly CanvasApiService _canvasApiService;
        private readonly CanvasCacheService _canvasCacheService;

        private readonly RubricService _rubricService;
        private readonly RubricDataService _rubricDataService;
        private readonly CourseTemplateService _courseTemplateService;

        private readonly ILogger<CourseController> _logger;

        public CourseController(CanvasApiService canvasApiService, CanvasCacheService canvasCacheService,
            RubricService rubricService, RubricDataService rubricDataService, CourseTemplateService courseTemplateService,
            ILogger<CourseController> logger)
        {
            _canvasApiService = canvasApiService;
            _canvasCacheService = canvasCacheService;
            _rubricService = rubricService;
            _rubricDataService = rubricDataService;
            _courseTemplateService = courseTemplateService;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var courses = await _canvasApiService.GetCourses();
            _canvasCacheService.SetCourses(courses);
            return View(courses);
        }

        public async Task<IActionResult> ViewAsync(int id)
        {
            ViewBag.Course = await _canvasCacheService.GetCourseAsync(id);

            var assignments = await _canvasApiService.GetAssignments(id);
            _canvasCacheService.SetAssignments(assignments);

            var lastImportTimes = new Dictionary<int, DateTime>();
            foreach (var assignment in assignments)
            {
                if (assignment.RubricSettings != null)
                    lastImportTimes[assignment.Id] = _rubricDataService.GetLastImportTime(assignment.Id.ToString());
            }
            ViewBag.LastImportTimes = lastImportTimes;

            // Skip GetRibrics(courseId) as it will raise a 403 error in some cases
            // (maybe because the user is a Co-Teacher instead of Teacher).
            // var hasNoRubric = (await _canvasApiService.GetRubrics(id)).Count == 0;
            // ViewBag.HasNoRubric = hasNoRubric;
            // if (hasNoRubric)
            //     ViewBag.CourseTemplates = _courseTemplateService.GetCourseTemplates();

            return View(assignments);
        }

        public async Task<IActionResult> PopulateFromTemplateAsync(int id, int templateId)
        {
            var rubricIdMap = new Dictionary<int, int>();
            var assignmentIdMap = new Dictionary<int, int>();
            var courseTemplate = _courseTemplateService.GetFullCourseTemplate(templateId);

            foreach (var assignmentTemplate in courseTemplate.AssignmentTemplates)
            {
                if (assignmentTemplate.Rubric != null)
                {
                    assignmentTemplate.Rubric.Criteria = _rubricService.GetCriteria(assignmentTemplate.Rubric.Id);
                    var rubricToCreate = new RubricForCreation(assignmentTemplate.Rubric, id);
                    var rubricCreated = await _canvasApiService.CreateRubric(rubricToCreate);
                    rubricIdMap[assignmentTemplate.Rubric.Id] = rubricCreated.Id;
                }
            }

            foreach (var assignmentTemplate in courseTemplate.AssignmentTemplates)
            {
                var assignmentToCreate = new AssignmentForCreation(assignmentTemplate, id);
                var assignmentCreated = await _canvasApiService.CreateAssignment(assignmentToCreate);
                assignmentIdMap[assignmentTemplate.Id] = assignmentCreated.Id;
            }

            return RedirectToAction("View", new { id });
        }

        public async Task<IActionResult> AssignPeerReviewsAsync(int id, int assignmentId)
        {
            // Canvas's "anonymous" peer review is kind of the opposite of what we want - instead of anonymous
            // reviewers, Canvas anonymizes reviewees, so we won't be using Canvas's peer review for our Teamwork
            // rubric assessment. With that said, I want to keep this action in case we need it for something
            // else, or in case Canvas adds the anonymous reviewer options in the future.
            // var groups = await _canvasApiService.GetGroups(id);
            var groups = new List<Group>();
            _logger.LogInformation("{groups} groups retrieved from course {course}", groups.Count, id);

            foreach (var group in groups)
            {
                var members = await _canvasApiService.GetGroupMemberships(group.Id);
                _logger.LogInformation("{members}/{memberCount} members retrieved from group {group}",
                    members.Count, group.MemberCount, group.Id);

                var memberIds = members.Select(m => m.UserId).ToList();
                foreach (var memberId in memberIds)
                {
                    var submission = await _canvasApiService.GetSubmission(id, assignmentId, memberId);
                    foreach (var otherId in memberIds)
                    {
                        if (memberId == otherId) continue;
                        var success = await _canvasApiService.CreatePeerReview(id, assignmentId, submission.Id, otherId);
                        if (!success)
                            _logger.LogError("Failed to create peer review for submission {submission}", submission.Id);
                    }
                    _logger.LogInformation("Finshed creating peers reviews for user {memberId}", memberId);
                }
            }

            return RedirectToAction("View", new { id });
        }
    }
}
