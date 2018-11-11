namespace SemanticLink.AspNetCore
{
    /// <summary>
    /// 
    /// </summary>
    /// <example>
    ///     Given that there are two routes throughout the code, grok the idea for either
    /// <code>
    ///
    ///  [HttpGet("{id}/tag/tagId", Name = TagUriFactory.TodoTagsRouteName)]
    ///  [HttpGet("tag/{id}", Name = TagUriFactory.TagRouteName)]
    /// 
    ///  // check that global tags exist in the todo set sent through as a uriList
    ///  var tagIds = uriList.ToTags(new List&lt;RouteAndParam&gt;
    ///     {
    ///       new RouteAndParam {Route = TagUriFactory.TodoTagRouteName, Param = "tagId"},
    ///       new RouteAndParam {Route = TagUriFactory.TagRouteName, Param = "id"},
    ///     },
    ///     HttpContext);
    /// </code>
    /// </example>
    public class RouteAndParam
    {
        /// <summary>
        ///     The route param setup on the Route
        /// </summary>
        /// <example>
        /// Route here would correspond to either `id` or `TagId` as required (both not both)
        /// <code>
        ///  [HttpGet("{id}/tag/tagId", Name = TagUriFactory.TodoTagsRouteName)]
        /// </code>
        /// </example>
        public string Param { get; set; } = "id";
        /// <summary>
        ///     A route name provided on the Route
        /// </summary>
        /// <example>
        /// Route here would correspond to the `Name` attribute
        /// <code>
        ///  [HttpGet("{id}/tag/tagId", Name = TagUriFactory.TodoTagsRouteName)]
        /// </code>
        /// </example>
        public string Route { get; set; }
    }
}