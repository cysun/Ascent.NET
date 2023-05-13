using System.Net.Http.Headers;
using Ascent.Areas.Canvas.Models;
using Ascent.Security;

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
        var accessToken = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == Constants.Claim.Cat)?.Value;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken ?? "");
        return await base.SendAsync(request, cancellationToken);
    }
}

// See https://canvas.instructure.com/doc/api/file.pagination.html for what a Link header looks like. In particular,
// it will not contain a "next" link if we are on the last page, and this is what we use to determine if we have
// exhausted all results.
public class Links
{
    public string Current { get; set; }
    public string Next { get; set; }
    public string Prev { get; set; }
    public string First { get; set; }
    public string Last { get; set; }

    public bool HasNext => Next != null;

    public static Links FromHttpResponse(HttpResponseMessage response)
    {
        var links = new Links();
        var header = response.Headers.GetValues("Link").FirstOrDefault();
        if (header != null)
        {
            var tokens = header.Split(',');
            foreach (var token in tokens)
            {
                var parts = token.Split(new char[] { '<', '>', ';', ' ', '"' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3) continue;

                switch (parts[2])
                {
                    case "current":
                        links.Current = parts[0]; break;
                    case "next":
                        links.Next = parts[0]; break;
                    case "prev":
                        links.Prev = parts[0]; break;
                    case "first":
                        links.First = parts[0]; break;
                    case "Last":
                        links.Last = parts[0]; break;
                    default:
                        break;
                }
            }
        }
        return links;
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

    public async Task<Assignment> GetAssignment(int courseId, int assignmentId) =>
        await _httpClient.GetFromJsonAsync<Assignment>($"courses/{courseId}/assignments/{assignmentId}");

    public async Task<Assignment> CreateAssignment(AssignmentForCreation assignment)
    {
        var uri = $"courses/{assignment.AssignmentProperty.CourseId}/assignments";
        return await PostAsJson<AssignmentForCreation, Assignment>(uri, assignment);
    }

    // The include[]=full_rubric_assessment parameter seems to be an "undocumented" feature as it's in the
    // documetation for the Get Single Submission API call but not in the List Submissions call.
    // Test it before use!!
    public async Task<List<Submission>> GetSubmissions(int courseId, int assignmentId)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("include[]", "full_rubric_assessment")
        };
        var queryString = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
        return await GetAll<Submission>($"courses/{courseId}/assignments/{assignmentId}/submissions?{queryString}");
    }

    public async Task<List<Models.Rubric>> GetRubrics(int courseId) =>
        await _httpClient.GetFromJsonAsync<List<Models.Rubric>>($"courses/{courseId}/rubrics");

    public async Task<Models.Rubric> CreateRubric(RubricForCreation rubric)
    {
        var uri = $"courses/{rubric.Association.AssociationId}/rubrics";
        return await PostAsJson<RubricForCreation, Models.Rubric>(uri, rubric);
    }

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

    private async Task<List<T>> GetAll<T>(string uri)
    {
        var results = new List<T>();
        var links = new Links { Next = uri };
        while (links.HasNext)
        {
            var response = await _httpClient.GetAsync(links.Next);
            if (!response.IsSuccessStatusCode)
            {
                var msg = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(msg);
            }
            results.AddRange(await response.Content.ReadFromJsonAsync<List<T>>());
            links = Links.FromHttpResponse(response);
        }
        return results;
    }

    private async Task<TReturn> PostAsJson<TValue, TReturn>(string uri, TValue value)
    {
        var response = await _httpClient.PostAsJsonAsync(uri, value);
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(msg);
        }
        return await response.Content.ReadFromJsonAsync<TReturn>();
    }
}
