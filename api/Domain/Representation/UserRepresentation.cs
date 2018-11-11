using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SemanticLink;

namespace Domain.Representation
{
    public class UserRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "email", Order = 40, EmitDefaultValue = false)] 
        public string Email { get; set; }

        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)] 
        public string Name { get; set; }

        [DataMember(Name = "externalIds", Order = 50, EmitDefaultValue = false)]
        public List<string> ExternalIds { get; set; }
                
        [DataMember(Name = "createdAt", Order = 100, EmitDefaultValue = false)]
        public DateTime CreatedAt { get; set; }
        
        [DataMember(Name = "updatedAt", Order = 100, EmitDefaultValue = false)]
        public DateTime UpdatedAt { get; set; }
    }
}