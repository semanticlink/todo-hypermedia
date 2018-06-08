using System;
using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    public class AuthenticateRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "token", Order = 50, EmitDefaultValue = false)]
        public string Token { get; set; }
                
        [DataMember(Name = "createdAt", Order = 100, EmitDefaultValue = false)]
        public DateTime CreatedAt { get; set; }
        
    }
}