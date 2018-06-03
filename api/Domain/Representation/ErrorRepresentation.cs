using Toolkit.LinkRelations;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    /// <summary>
    ///     Used for returning error pages, such as 401, so that we can return an html representation for the humble browser test
    /// </summary>
    public class ErrorRepresentation : LinkedRepresentation
    {
        /// <summary>
        ///     Helper to create with 'self' link
        /// </summary>
        public static ErrorRepresentation MakeSparse(string selfUri)
        {
            return new ErrorRepresentation
            {
                Links = new[]
                {
                    new WebLink
                    {
                        Rel = IanaLinkRelation.Self,
                        HRef = selfUri
                    }
                }
            };
        }
    }
}