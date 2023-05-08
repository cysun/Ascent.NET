using Ascent.Areas.Canvas.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Ascent.Areas.Canvas.Services;

// We will cache Canvas data like courses and assignments to avoid repeated calls to
// Canvas API. This not only improves performance, but also reduce the chance of
// being throttled by Canvas.
public class CanvasCacheService
{
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions defaultOptions;

    private readonly CanvasApiService _canvasApiService;

    public CanvasCacheService(IMemoryCache cache, CanvasApiService canvasApiService)
    {
        _cache = cache;
        _canvasApiService = canvasApiService;
        defaultOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
    }

    private string CourseKey(int id) => $"course-{id}";
    private string AssignmentKey(int id) => $"assignment-{id}";

    public void SetCourse(Course course) => _cache.Set(CourseKey(course.Id), course, defaultOptions);

    public void SetCourses(IEnumerable<Course> courses)
    {
        foreach (var course in courses) SetCourse(course);
    }

    public async Task<Course> GetCourseAsync(int id) => (Course)_cache.Get(CourseKey(id)) ?? await _canvasApiService.GetCourse(id);

    public void SetAssignment(Assignment assignment) => _cache.Set(AssignmentKey(assignment.Id), assignment, defaultOptions);

    public void SetAssignments(IEnumerable<Assignment> assignments)
    {
        foreach (var assignment in assignments) SetAssignment(assignment);
    }

    public async Task<Assignment> GetAssignmentAsync(int courseId, int assignmentId) =>
        (Assignment)_cache.Get(AssignmentKey(assignmentId)) ?? await _canvasApiService.GetAssignment(courseId, assignmentId);
}
