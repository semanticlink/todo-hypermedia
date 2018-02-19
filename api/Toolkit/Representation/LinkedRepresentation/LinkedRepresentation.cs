using System.Runtime.Serialization;

namespace Toolkit.Representation.LinkedRepresentation
{
    [DataContract(Name = "linked")]
    public abstract class LinkedRepresentation
    {
        [DataMember(Name = "links", Order = 10, EmitDefaultValue = false)]
        public WebLink[] Links { get; set; }
    }
}