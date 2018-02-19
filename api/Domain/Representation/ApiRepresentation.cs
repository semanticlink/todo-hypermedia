using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    /// <summary>
    ///     The API is a link of links to resources
    /// </summary>
    [DataContract(Name = "api")]
    public class ApiRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "version", Order = 20)]
        public string Version { get; set; }
    }
}