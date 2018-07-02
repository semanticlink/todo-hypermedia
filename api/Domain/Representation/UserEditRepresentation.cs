using System.Collections.Generic;
using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    public class UserEditData : LinkedRepresentation
    {
        [DataMember(Name = "email", Order = 40, EmitDefaultValue = false)] 
        public string Email { get; set; }

        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)] 
        public string Name { get; set; }

        [DataMember(Name = "externalIds", Order = 50, EmitDefaultValue = false)]
        public List<string> ExternalIds { get; set; }
                
    }
}