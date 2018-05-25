﻿using System.Runtime.Serialization;

namespace Domain.Representation
{
    public class UserCreateDataRepresentation
    {
        [DataMember(Name = "email", Order = 40, EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "password", Order = 50, EmitDefaultValue = false)]
        public string Password { get; set; }
    }
}