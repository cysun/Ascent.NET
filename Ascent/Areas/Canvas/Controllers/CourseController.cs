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
