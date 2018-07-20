using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "form-item")]
    public abstract class FormItemRepresentation
    {
        [DataMember(Name = "type", Order = 20)]
        public string Type { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false, Order = 30)]
        public string Id { get; set; }

        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "required", EmitDefaultValue = false, Order = 50)]
        public bool Required { get; set; }

        [DataMember(Name = "description", EmitDefaultValue = false, Order = 70)]
        public string Description { get; set; }

        /// <summary>
        ///     Whether multiple of the items are available. This translates to the data type
        ///     being a collection (array).
        /// </summary>
        [DataMember(Name = "multiple", Order = 20, EmitDefaultValue = false)]
        public bool Multiple { get; set; }
    }
}