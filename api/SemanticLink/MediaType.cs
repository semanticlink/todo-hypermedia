namespace SemanticLink
{
    public static class MediaType
    {
        /// <summary>
        ///     The text/uri-list Internet Media Type is defined to provide a simple format for
        ///     the automatic processing of such lists of URIs. see https://tools.ietf.org/html/rfc2483#section-5
        /// </summary>
        /// <remarks>
        ///    see http://amundsen.com/hypermedia/urilist/
        /// </remarks>
        /// <example>
        ///     <code>
        ///         # a todo tag on todo/345
        ///         https://example.com/345/tag/45
        ///         # another (necessarily on the same todo)
        ///         https://example.com/345/tag/454
        ///     </code>
        /// </example>
        public const string UriList = "text/uri-list";

        /// <summary>
        ///     JSON Patch is a web standard format for describing changes in a JSON document. It is meant to
        ///     be used together with HTTP Patch which allows for the modification of existing HTTP resources.
        ///     see https://tools.ietf.org/html/rfc6902
        /// </summary>
        /// <remarks>
         /// <para>
        ///     A <see cref="FeedRepresentation"/> can't be patched using JSON Merge Patch specification because of how it
        ///     works with arrays in JSON (it is either a DELETE *the* collection or PUT on collection). We'll
        ///     need to use JSON Patch to show add or remove from collection.
        ///</para>
        /// <para>
        ///     For this collection, it is a collection of READONLY items (ie global tags). So we won't be updating details
        ///     on a resource.
        ///</para>
        /// <para>
        /// <b>Remove:</b> however, our <see cref="FeedItemRepresentation"/> needs to be able to remove items by <see cref="FeedItemRepresentation.Id"/>
        ///
        ///     In this case, we have two options based on JSON Pointer (rfc9601):
        ///
        ///    <li>implement array index '/item/0' (given that feed do not guarantee 'natural' order)</li>
        ///    <li>extended syntax (to be implemented)'/items[id="https://example.com/tag/xxxx"]'</li>
        /// Example
        ///    <code>
        ///       [
        ///          { "op": "remove", "path": "/items/0" },
        ///          { "op": "remove", "path": "/items[id='https://example.com/tag/xxxx']" },
        ///       ]
        ///     </code>
        /// </para>
        /// <para>
        ///    <b>Add:</b> is much easier but the syntax has gotchas
        ///
        ///     <li>Adding to a list include '-' </li>
        /// Example
        ///    <code>
        ///       [
        ///          { "op": "add", "path": "/items/-", "value": { id: "https://example.com/tag/yyyy" } }
        ///       ]
        ///     </code>
        /// </para>
        ///     <li>see https://tools.ietf.org/html/rfc7396 (JSON Merge Patch)</li>
        ///     <li>see https://tools.ietf.org/html/rfc6902 (JSON Patch)</li>
        ///     <li>see https://tools.ietf.org/html/rfc6901#section-7 (JSON Pointer)</li>
        ///     <li>good examples http://benfoster.io/blog/aspnet-core-json-patch-partial-api-updates
        ///         and http://jsonpatch.com/
        ///     </li>
        /// </remarks>
       public const string JsonPatch = "application/json-patch+json";
    }
}