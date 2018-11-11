namespace SemanticLink.AspNetCore
{
    public static class CacheDuration
    {
        public const int Short = 20;
        public const int NotLong = 60;
        public const int Long = 3600;
        public const int Forever = 3600 * 60;
    }
}