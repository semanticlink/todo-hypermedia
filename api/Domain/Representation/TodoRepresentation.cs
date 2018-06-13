using System;
using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    public class TodoRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "completed", Order = 50, EmitDefaultValue = false)]
        public bool Completed { get; set; }

        [DataMember(Name = "state", Order = 50, EmitDefaultValue = false)]
        public string State { get; set; }

        [DataMember(Name = "due", Order = 50, EmitDefaultValue = false)]
        public DateTime Due { get; set; }
    }
}