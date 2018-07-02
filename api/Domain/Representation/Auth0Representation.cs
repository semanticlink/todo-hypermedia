using System.Collections.Generic;
using System.ComponentModel;
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
    [Description("Authorisation config for Auth0. see https://auth0.com/docs/libraries/auth0js/v9")]
    public class Auth0Representation : LinkedRepresentation
    {
        [DataMember(Name = "clientID", Order = 40, EmitDefaultValue = true)]
        [Description("Your Auth0 client ID.")]
        public string ClientId { get; set; }

        [DataMember(Name = "domain", Order = 50, EmitDefaultValue = true)]
        [Description("The default audience to be used for requesting API access.")]
        public string Domain { get; set; }

        [DataMember(Name = "audience", Order = 50, EmitDefaultValue = true)]
        [Description("The default audience to be used for requesting API access.")]
        public string Audience { get; set; }

        [DataMember(Name = "scope", Order = 100, EmitDefaultValue = false)]
        [Description("The scopes which you want to request authorization for. These must be separated by a space. You can request any of the standard OIDC scopes about users, such as profile and email, custom claims that must conform to a namespaced format, or any scopes supported by the target API (for example, read:contacts). Include offline_access to get a Refresh Token.")]
        public List<string> RequestedScopes { get; set; }

        [DataMember(Name = "leeway", Order = 100, EmitDefaultValue = false)]
        [Description("A value in seconds; leeway to allow for clock skew with regard to JWT expiration times.")]
        public int Leeway { get; set; }

        [DataMember(Name = "responseType", Order = 100, EmitDefaultValue = false)]
        [Description("It can be any space separated list of the values code, token, id_token. It defaults to 'token', unless a redirectUri is provided, then it defaults to 'code'.")]
        public List<string> ResponseType { get; set; }

        [DataMember(Name = "realm", Order = 100, EmitDefaultValue = false)]
        [Description("The realm to be matched with www-authenticate header. Default is 'api-auth0'")]
        public string Realm { get; set; } = "api-auth0";
    }
}