using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Ascent.Security;

public class CanvasSettings
{
    public string Domain { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }

    public string AuthorizationEndpoint => $"{Domain}/login/oauth2/auth";
    public string TokenEndpoint => $"{Domain}/login/oauth2/token";

    public string ApiBaseUrl => $"{Domain}/api/v1/";
}

public class CanvasTokens
{
    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public string TokenType { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public static async Task<CanvasTokens> FromHttpContextAsync(HttpContext context)
    {
        var accessToken = await context.GetTokenAsync(Constants.AuthenticationScheme.CanvasCookie, "access_token");
        if (accessToken == null)
            return null;

        var canvasTokens = new CanvasTokens
        {
            AccessToken = accessToken,
            RefreshToken = await context.GetTokenAsync(Constants.AuthenticationScheme.CanvasCookie, "refresh_token"),
            TokenType = await context.GetTokenAsync(Constants.AuthenticationScheme.CanvasCookie, "token_type"),
            ExpiresAt = DateTimeOffset.Parse(await context.GetTokenAsync(Constants.AuthenticationScheme.CanvasCookie, "expires_at")).LocalDateTime
        };

        return canvasTokens;
    }
}

public class CanvasUtils
{
    // Based on Auth0WebAppWithAccessTokenAuthenticationBuilder from https://github.com/auth0/auth0-aspnetcore-authentication and
    // https://stackoverflow.com/questions/60858985/addopenidconnect-and-refresh-tokens-in-asp-net-core
    public static Func<CookieValidatePrincipalContext, Task> OnValidatePrincipal(CanvasSettings canvasSettings)
    {
        return async context =>
        {
            var accessToken = context.Properties.GetTokenValue("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                var refreshToken = context.Properties.GetTokenValue("refresh_token");
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    var now = DateTimeOffset.Now;
                    var expiresAt = DateTimeOffset.Parse(context.Properties.GetTokenValue("expires_at")!);
                    var leeway = 2;
                    var difference = DateTimeOffset.Compare(expiresAt, now.AddMinutes(leeway));
                    var isExpired = difference <= 0;

                    if (isExpired)
                    {
                        var result = await (new TokenClient()).Refresh(canvasSettings, refreshToken);
                        if (result != null)
                        {
                            context.Properties.UpdateTokenValue("access_token", result.AccessToken);
                            if (!string.IsNullOrEmpty(result.RefreshToken))
                            {
                                context.Properties.UpdateTokenValue("refresh_token", result.RefreshToken);
                            }
                            context.Properties.UpdateTokenValue("expires_at", DateTimeOffset.Now.AddSeconds(result.ExpiresIn).ToString("o"));
                            context.ShouldRenew = true;
                        }
                        else
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync();
                        }
                    }
                }
            }
        };
    }

    // Taken from Utils from https://github.com/auth0/auth0-aspnetcore-authentication
    public static Func<T, Task> ProxyEvent<T>(Func<T, Task> newHandler, Func<T, Task> originalHandler)
    {
        return async (context) =>
        {
            if (newHandler != null)
            {
                await newHandler(context);
            }
            if (originalHandler != null)
            {
                await originalHandler(context);
            }
        };
    }
}

// Based on AcccessTokenResponse from https://github.com/auth0/auth0-aspnetcore-authentication
// The actual Canvas token response has a few other fields, but we only need these three.
internal class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

// TokenClient from https://github.com/auth0/auth0-aspnetcore-authentication
internal class TokenClient
{
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<TokenResponse> Refresh(CanvasSettings settings, string refreshToken)
    {
        var body = new Dictionary<string, string> {
                { "grant_type", "refresh_token" },
                { "client_id", settings.ClientId },
                { "client_secret", settings.ClientSecret },
                { "refresh_token", refreshToken }
            };

        var requestContent = new FormUrlEncodedContent(body.Select(p => new KeyValuePair<string, string>(p.Key, p.Value ?? "")));

        using (var request = new HttpRequestMessage(HttpMethod.Post, settings.TokenEndpoint) { Content = requestContent })
        {
            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                    return null;

                var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return await JsonSerializer.DeserializeAsync<TokenResponse>(contentStream).ConfigureAwait(false);
            }
        }
    }
}
