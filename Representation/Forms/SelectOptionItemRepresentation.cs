using System.Runtime.Serialization;

namespace TodoApi.Representation.Forms
{
    [DataContract(Name = "select-option-form-item")]
    public abstract class SelectOptionItemRepresentation : FormItemRepresentation
    {
        [DataMember(Name = "selected", Order = 10, EmitDefaultValue = false)]
        public bool Selected { get; set; }
    }
}