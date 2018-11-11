using System;
using System.Runtime.Serialization;
using SemanticLink;

namespace Domain.Representation
{
    public class TenantRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "code", Order = 50, EmitDefaultValue = false)]
        public string Code { get; set; }
        
        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }
        
        [DataMember(Name = "description", Order = 50, EmitDefaultValue = false)]
        public string Description { get; set; }
        
        [DataMember(Name = "createdAt", Order = 100, EmitDefaultValue = false)]
        public DateTime CreatedAt { get; set; }
        
        [DataMember(Name = "updatedAt", Order = 100, EmitDefaultValue = false)]
        public DateTime UpdatedAt { get; set; }
    }
}