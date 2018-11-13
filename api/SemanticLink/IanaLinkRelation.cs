namespace SemanticLink
{
    /// See http://en.wikipedia.org/wiki/Link_relation
    /// See http://www.iana.org/assignments/link-relations/link-relations.xhtml
    public static class IanaLinkRelation
    {
        /// <summary>
        ///     Conveys an identifier for the link's context.
        /// </summary>
        /// <remarks>
        ///    http://www.iana.org/go/rfc4287
        /// </remarks>
        public const string Self = "self";

        /// <summary>
        ///    Refers to a parent document in a hierarchy of documents.
        /// </summary>
        /// <remarks>
        ///    http://www.iana.org/go/rfc8288
        /// </remarks>
        public const string Up = "up";

        /// <summary>
        ///     Refers to a resource that can be used to search through the link's context and related resources.
        /// </summary>
        /// <remarks>
        ///    http://www.opensearch.org/Specifications/OpenSearch/1.1
        /// </remarks>
        public const string Search = "search";

        /// <summary>
        ///     Refers to an icon representing the link's context.
        /// </summary>
        /// <remarks>
        ///    http://www.w3.org/TR/html5/links.html#link-type-icon
        /// </remarks>
        public const string Icon = "icon";

        /// <summary>
        ///    The target IRI points to a resource where a submission form can be obtained.
        /// </summary>
        /// <remarks>
        ///    http://www.iana.org/go/rfc6861
        /// </remarks>
        public const string CreateForm = "create-form";

        /// <summary>
        ///    The target IRI points to a resource where a submission form for editing associated resource can be obtained.
        /// </summary>
        /// <remarks>
        ///    http://www.iana.org/go/rfc6861
        /// </remarks>
        public const string EditForm = "edit-form";

        /// <summary>
        ///    Designates the preferred version of a resource (the IRI and its contents).
        /// </summary>
        /// <remarks>
        ///    http://www.iana.org/go/rfc6596
        /// </remarks>
        public const string Canonical = "canonical";

        /// <summary>
        ///    Indicates a resource where payment is accepted.
        /// </summary>
        /// <remarks>
        ///    http://www.iana.org/go/rfc8288
        /// </remarks>
        public const string Payment = "payment";

        /// <summary>
        ///    Points to a resource containing the version history for the context
        /// </summary>
        /// <remarks>
        ///    http://www.iana.org/go/rfc5829
        /// </remarks>
        public const string VersionHistory = "version-history";

        /// <summary>
        ///    	Refers to a substitute for this context
        /// </summary>
        /// <remarks>
        ///    http://www.w3.org/TR/html5/links.html#link-type-alternate
        /// </remarks>
        public const string Alternate = "alternate";

        /// <summary>
        ///     Identifying that a resource representation conforms to a certain profile, without affecting the
        ///     non-profile semantics of the resource representation.
        /// <</summary>
        /// <remarks>
        ///     https://tools.ietf.org/html/rfc6906
        /// </remarks>
        public const string Profile = "profile";
    }
}