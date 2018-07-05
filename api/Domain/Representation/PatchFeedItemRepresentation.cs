using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    /// <summary>
    ///     Server-side representation of a <see cref="FeedItemRepresentation"/> used with the <see cref="JsonPatchDocument"/>
    ///     because it does not support the use of Arrays that we have in <see cref="FeedRepresentation"/>.
    /// </summary>
    /// <remarks>
    ///    This is temporary and for demonstration purposes.
    /// </remarks>
    /// <see cref="FeedItemRepresentation"/>
    public class PatchFeedItemRepresentation
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}