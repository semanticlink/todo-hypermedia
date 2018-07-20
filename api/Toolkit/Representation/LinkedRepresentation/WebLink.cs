using System.Runtime.Serialization;

namespace Toolkit.Representation.LinkedRepresentation
{
    /// <summary>
    ///     An agnostic content type as a means of indicating the relationship
    ///     between resources on the web. It is a subset of the Web Linking RFC 5988
    ///     see https://tools.ietf.org/html/rfc5988
    /// </summary>
    [DataContract(Name = "link")]
    public class WebLink
    {
        /// <summary>
        ///     The descriptive name name in order to define the type of link or the relationship.
        ///     This is the fundamental part of the semantic interface where a client
        ///     follows the link based on the relationship between the source and
        ///     destination resources. Example link relation types are found at
        ///     http://www.iana.org/assignments/link-relations/link-relations.xhtml
        /// </summary>
        [DataMember(Name = "rel", Order = 10, EmitDefaultValue = false)]
        public string Rel { get; set; }

        /// <summary>
        ///     The URI to the target link. See http://tools.ietf.org/html/rfc5988#section-5.1
        /// </summary>
        [DataMember(Name = "href", Order = 20, EmitDefaultValue = false)]
        public string HRef { get; set; }

        /// <summary>
        ///     Human readable label for the destination. This may from part of the semantic
        ///     interface where the link relation (rel) is insufficient or ambiguous
        /// </summary>
        [DataMember(Name = "title", Order = 30, EmitDefaultValue = false)]
        public string Title { get; set; }
 
        /// <summary>
        ///     The "type" parameter, when present, is a hint indicating what the
        ///     media type of the result of dereferencing the link should be.  Note
        ///     that this is only a hint; for example, it does not override the
        ///     Content-Type header of a HTTP response obtained by actually following
        ///     the link.
        /// </summary>
        [DataMember(Name = "type", Order = 40, EmitDefaultValue = false)]
        public string Type { get; set; }
    }
}