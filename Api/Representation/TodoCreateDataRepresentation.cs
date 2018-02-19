using System;
using System.Runtime.Serialization;

namespace TodoApi.Representation
{
    public class TodoCreateDataRepresentation
    {
        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "completed", Order = 50, EmitDefaultValue = false)]
        public bool Completed { get; set; }

        [DataMember(Name = "due", Order = 50, EmitDefaultValue = false)]
        public DateTime Due { get; set; }
    }
}