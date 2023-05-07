using System.Net.Http.Headers;
using Ascent.Areas.Canvas.Models;
using Ascent.Security;
using Microsoft.AspNetCore.Authentication;

namespace Ascent.Areas.Canvas.Services;

// TokenHandler from https://github.com/auth0-blog/refresh-token-aspnet-core
public class CanvasHttpMessageHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CanvasHttpMessageHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(Constants.AuthenticationScheme.CanvasCookie, "access_token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await base.SendAsync(request, cancellationToken);
    }
}

public class CanvasApiService
{
    private readonly HttpClient _httpClient;

    public CanvasApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("CanvasAPI");
    }

    public async Task<List<Course>> GetCourses(string enrollmentType = EnrollmentType.Teacher)
    {
        // The reason to use List<KeyValuePair> instead of the more concise Dictionary<string,string> is that
        // we may have multiple parameters with the same name/key, e.g. include[].
        var parameters = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("enrollment_type", enrollmentType),
            new KeyValuePair<string, string>("enrollment_state", "active"),
            new KeyValuePair<string, string>("state", "available"),
            new KeyValuePair<string, string>("include[]", "term"),
            new KeyValuePair<string, string>("per_page", "20")
        };
        var queryString = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
        return await _httpClient.GetFromJsonAsync<List<Course>>($"courses?{queryString}");
    }

    public async Task<Course> GetCourse(int id)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("include[]", "term")
        };
        var queryString = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
        return await _httpClient.GetFromJsonAsync<Course>($"courses/{id}?{queryString}");
    }

    public async Task<List<Assignment>> GetAssignments(int courseId) =>
        await _httpClient.GetFromJsonAsync<List<Assignment>>($"courses/{courseId}/assignments");

    public async Task<List<Group>> GetGroups(int courseId)
    {
        var parameters = new Dictionary<string, string>
        {
            { "per_page", "20"}
        };
        var queryString = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
        return await _httpClient.GetFromJsonAsync<List<Group>>($"courses/{courseId}/groups?{queryString}");
    }

    public async Task<List<GroupMembership>> GetGroupMemberships(int groupId) =>
        await _httpClient.GetFromJsonAsync<List<GroupMembership>>($"groups/{groupId}/memberships");

    public async Task<Submission> GetSubmission(int courseId, int assignmentId, int userId) =>
        await _httpClient.GetFromJsonAsync<Submission>($"courses/{courseId}/assignments/{assignmentId}/submissions/{userId}");

    public async Task<bool> CreatePeerReview(int courseId, int assignmentId, int submissionId, int userId)
    {
        var parameters = new Dictionary<string, string>
        {
            { "user_id", userId.ToString()}
        };
        var response = await _httpClient.PostAsync($"courses/{courseId}/assignments/{assignmentId}/submissions/{submissionId}/peer_reviews",
            new FormUrlEncodedContent(parameters));

        return response.IsSuccessStatusCode;
    }
}
