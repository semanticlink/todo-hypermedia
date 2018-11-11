using System.Collections.Generic;
using SemanticLink;

namespace Domain.Representation
{
    /// <summary>
    ///     Server-side representation of a <see cref="FeedRepresentation"/> used with the <see cref="JsonPatchDocument"/>
    ///     because it does not support the use of Arrays that we have in <see cref="FeedRepresentation"/>.
    /// </summary>
    /// <remarks>
    ///    This is temporary and for demonstration purposes.
    /// </remarks>
    /// <see cref="FeedRepresentation"/>
    public class PatchFeedRepresentation
    {
        public IList<PatchFeedItemRepresentation> Items { get; set; }
    }
}