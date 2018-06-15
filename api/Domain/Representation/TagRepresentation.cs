using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    public class TagRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }
    }
}