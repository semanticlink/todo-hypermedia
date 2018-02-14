using System.Runtime.Serialization;

namespace TodoApi.Representation
{
    /// <summary>
    ///     The API is a link of links to resources
    /// </summary>
    [DataContract(Name = "api")]
    public class ApiRepresentation : LinkedRepresentation.LinkedRepresentation
    {
        [DataMember(Name = "version", Order = 20)]
        public string Version { get; set; }
    }
}