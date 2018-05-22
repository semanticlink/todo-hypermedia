using System;
using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    public class UserRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "email", Order = 40, EmitDefaultValue = false)] 
        public string Email { get; set; }

        [DataMember(Name = "password", Order = 50, EmitDefaultValue = false)] 
        public string Password { get; set; }
                
        [DataMember(Name = "createdAt", Order = 100, EmitDefaultValue = false)]
        public DateTime CreatedAt { get; set; }
        
        [DataMember(Name = "updatedAt", Order = 100, EmitDefaultValue = false)]
        public DateTime UpdatedAt { get; set; }
    }
}