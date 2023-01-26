namespace Ascent.Security;

public static class Constants
{
    public static class Claim
    {
        public const string Read = "ascent_read";
        public const string Write = "ascent_write";
        public const string Project = "ascent_project";
    }

    public static class Policy
    {
        public const string CanRead = "CanRead";
        public const string CanWrite = "CanWrite";
        public const string CanManageProject = "CanManageProject";
    }
}
