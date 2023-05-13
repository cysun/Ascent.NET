namespace Ascent.Security;

public static class Constants
{
    public static class AuthenticationScheme
    {
        public const string Oidc = "Oidc";
        public const string Canvas = "Canvas";
        public const string CanvasCookie = "CanvasCookie";
    }

    public static class Claim
    {
        public const string Read = "ascent_read";
        public const string Write = "ascent_write";
        public const string Project = "ascent_project";
        public const string Cat = "canvas_access_token";
    }

    public static class Policy
    {
        public const string CanRead = "CanRead";
        public const string CanWrite = "CanWrite";
        public const string CanManageProject = "CanManageProject";
        public const string HasCat = "HasCat";
    }

    public static class Canvas
    {
        public const string AccessToken = "access_token";
        public const string RefreshToken = "refresh_token";
        public const string TokenType = "token_type";
        public const string ExpiresAt = "expires_at";
    }
}
