using Ascent.Areas.Canvas.Models;
using Ascent.Security;
using Ascent.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Ascent.Areas.Canvas.Controllers
{
    [Area("Canvas")]
    [Authorize(AuthenticationSchemes = Constants.AuthenticationScheme.Canvas)]
    public class CourseController : Controller
    {
        private readonly CanvasApiService _canvasApiService;

        private readonly IMemoryCache _memoryCache;

        private readonly ILogger<CourseController> _logger;

        private const string CoursesCacheKey = "_courses";

        public CourseController(CanvasApiService canvasApiService, IMemoryCache memoryCache, ILogger<CourseController> logger)
        {
            _canvasApiService = canvasApiService;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var courses = await _canvasApiService.GetCourses();
            PutCoursesInCache(courses);
            return View(courses);
        }

        public async Task<IActionResult> ViewAsync(int id)
        {
            if (!GetCoursesFromCache().TryGetValue(id, out var course))
            {
                course = new Course
                {
                    Id = id,
                    Name = $"Course with Id={id}"
                };
            }

            ViewBag.Course = course;

            var assignments = await _canvasApiService.GetAssignments(id);
            return View(assignments);
        }

        public async Task<IActionResult> AssignPeerReviewsAsync(int id, int assignmentId)
        {
            var groups = await _canvasApiService.GetGroups(id);
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

        private Dictionary<int, Course> GetCoursesFromCache()
        {
            if (!_memoryCache.TryGetValue(CoursesCacheKey, out Dictionary<int, Course> courses))
            {
                courses = new Dictionary<int, Course>();
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _memoryCache.Set(CoursesCacheKey, courses, cacheEntryOptions);
            }

            return courses;
        }

        private void PutCoursesInCache(List<Course> courses)
        {
            var cachedCourese = GetCoursesFromCache();
            foreach (var course in courses)
            {
                if (!cachedCourese.ContainsKey(course.Id))
                    cachedCourese.Add(course.Id, course);
            }
        }
    }
}
