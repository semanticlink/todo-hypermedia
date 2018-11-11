using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "form")]
    public class FormRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "items", Order = 100)]
        public FormItemRepresentation[] Items { get; set; }
    }
}