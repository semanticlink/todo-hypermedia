using System.Runtime.Serialization;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    /// <summary>
    /// <para>
    ///    Auth0 configuration representation for the client to sent to auth0 to authenticate with
    /// </para>
    ///
    /// <example>
    ///
    ///    clientID: '3CYUtb8Uf9NxwesvBJAs2gNjqYk3yfZ8',
    ///    domain: 'rewire-sample.au.auth0.com',
    ///    audience: 'todo-rest-test'
    ///
    /// </example>
    /// </summary>
    public class Auth0Representation : LinkedRepresentation
    {
        [DataMember(Name = "clientID", Order = 40, EmitDefaultValue = false)]
        public string ClientId { get; set; }

        [DataMember(Name = "domain", Order = 50, EmitDefaultValue = false)]
        public string Domain { get; set; }

        [DataMember(Name = "audience", Order = 100, EmitDefaultValue = false)]
        public string Audience { get; set; }

        [DataMember(Name = "scope", Order = 100, EmitDefaultValue = false)]
        public string RequestedScopes { get; set; }

        [DataMember(Name = "leeway", Order = 100, EmitDefaultValue = false)]
        public int Leeway { get; set; }

        [DataMember(Name = "responseType", Order = 100, EmitDefaultValue = false)]
        public string ResponseType { get; set; }
    }
}