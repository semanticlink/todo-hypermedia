using App;

namespace Api.Authorisation
{
    /// <summary>
    ///     Constants for route params in Uris. These are used to be able to pick out of the RouteData.
    /// </summary>
    /// <example>
    ///    [HttpGet("user/{id}"]      --> Id ("id")
    ///    [HttpGet("user/{id:int}"]  --> Id ("id")
    ///    [HttpGet("user/{id?}"]     --> Id ("id")
    ///    [HttpGet("user/"]          --> Root ("/")
    /// </example>
    public static class ResourceKey
    {
        /// <summary>
        ///     The 'id' route param
        /// </summary>
        public const string Id = "id";

        /// <summary>
        ///     Top level collections. Hence they won't have a resource Id to get. But we already know what it is.
        /// </summary>
        /// <seealso cref="TrustDefaults.KnownHomeResourceId"/>
        public const string Root = "/";

        /// <summary>
        ///     User level collections. Hence they won't have a resource Id to get. So we need to pick up the current user.
        /// </summary>
        public const string User = "userId";
    }
}