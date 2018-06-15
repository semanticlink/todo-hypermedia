using System.Runtime.Serialization;

namespace Domain.Representation
{
    public class TagCreateDataRepresentation
    {
        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }
    }
}