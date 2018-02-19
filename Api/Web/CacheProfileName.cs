namespace Api.Web
{
    /// <summary>
    ///     The names of cache files registered with ASP.NET Core MVC for use on controllers.
    /// </summary>
    /// <remarks>
    ///     Use this on your controllers or methods like:
    ///     <code>
    ///       [ResponseCache(CacheProfileName = CacheProfileName.NoCache)]
    ///     </code>
    /// </remarks>
    public static class CacheProfileName
    {
        /// <summary>
        ///     This profile is used to mark the response as DO NOT CACHE (at all).
        /// </summary>
        public const string NoCache = "NoCache";

        /// <summary>
        ///     Use this profile for images off an API, which should only be
        ///     cached by a client (i.e. they are <b>not</b> public images.)
        /// </summary>
        public const string PrivateImage = "PrivateImage";

        /// <summary>
        ///     Use this profile for API method that return immutable data, which should only be
        ///     cached by a client (i.e. they are <b>not</b> public.)
        /// </summary>
        /// <remarks>
        /// </remarks>
        public const string PrivateApiImmutableData = "PrivateImmutableData";

        /// <summary>
        ///     A private api resource that can be cached.
        /// </summary>
        /// <remarks>
        ///     The resource is marked as 'private', so intermediate cached will
        ///     no cache this value. The resource is not marked as 'nostore'  as the
        ///     client can store it as long as they like (.i.eit doesn't have credit
        ///     card information or other highly sensitive data).
        /// </remarks>
        public const string PrivateApi = "PrivateApi";

        /// <summary>
        ///     An API resource that can be cached for a medium time frame (around an hour).
        /// </summary>
        /// <remarks>
        ///     The resource is marked as 'private', so intermediate caches will
        ///     no cache this value. The resource is not marked as 'nostore' as the
        ///     client can store it as long as they like (i.e. it doesn't have credit
        ///     card information or other highly sensitive data).
        /// </remarks>
        public const string Private1HourApi = "PrivateMediumApi";

        /// <summary>
        ///     An API resource that can be cached for a medium time frame (around an hour).
        /// </summary>
        /// <remarks>
        ///     The resource is marked as 'private', so intermediate caches will
        ///     no cache this value. The resource is not marked as 'nostore' as the
        ///     client can store it as long as they like (i.e. it doesn't have credit
        ///     card information or other highly sensitive data).
        /// </remarks>
        public const string Private1MinuteApi = "PrivateShortApi";

        /// <summary>
        ///     The resource is public and can be freely cached and shared for a short
        ///     period of time before it becomes invalid.
        /// </summary>
        public const string PublicShort = "PublicShort";
    }
}