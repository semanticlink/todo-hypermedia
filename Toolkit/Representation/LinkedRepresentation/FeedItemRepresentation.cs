using System;
using System.Runtime.Serialization;

namespace Toolkit.Representation.LinkedRepresentation
{
    [DataContract(Name = "feed-item")]
    public class FeedItemRepresentation : LinkedRepresentation
    {
        /// <summary>
        ///     The canonical URI for the item.
        /// </summary>
        [DataMember(Name = "id", Order = 15, EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "title", Order = 20, EmitDefaultValue = false)]
        public string Title { get; set; }

        [DataMember(Name = "published", Order = 30, EmitDefaultValue = false)]
        public DateTime? Published { get; set; }

        [DataMember(Name = "updated", Order = 40, EmitDefaultValue = false)]
        public DateTime? Updated { get; set; }

        [DataMember(Name = "author", Order = 50, EmitDefaultValue = false)]
        public string Author { get; set; }

        [DataMember(Name = "categories", Order = 50, EmitDefaultValue = false)]
        public string[] Categories { get; set; }
    }
}