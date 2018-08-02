using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    public class TenantCreateDataRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "code", Order = 50, EmitDefaultValue = false)]
        public string Code { get; set; }

        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "description", Order = 50, EmitDefaultValue = false)]
        public string Description { get; set; }
    }
}