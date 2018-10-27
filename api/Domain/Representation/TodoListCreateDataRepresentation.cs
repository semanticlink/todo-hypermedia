using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    public class TodoListCreateDataRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }
       
        [DataMember(Name = "tenant", Order = 100, EmitDefaultValue = false)]
        public string Tenant { get; set; }
    }
}