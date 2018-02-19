using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "form")]
    public class FormRepresentation : LinkedRepresentation.LinkedRepresentation
    {
        [DataMember(Name = "items", Order = 20)]
        public FormItemRepresentation[] Items { get; set; }
    }
}