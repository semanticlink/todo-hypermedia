using System.Runtime.Serialization;
using TodoApi.Representation.LinkedRepresentation;

namespace TodoApi.Representation.Forms
{
    [DataContract(Name = "select-option-form-item")]
    public class SelectOptionGroupItemRepresentation : SelectOptionItemRepresentation
    {
        /// <summary>
        ///     A recursive group or value.
        /// </summary>
        [DataMember(Name = "items", Order = 100)]
        public SelectOptionItemRepresentation[] Items { get; set; }

        /// <summary>
        ///     A reference to a resource that can provide a feed of values
        ///     that can be selected.
        /// </summary>
        [DataMember(Name = "link", Order = 20, EmitDefaultValue = false)]
        public WebLink Link { get; set; }
    }
}