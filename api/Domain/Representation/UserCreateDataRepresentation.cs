using System.ComponentModel;
using System.Runtime.Serialization;

namespace Domain.Representation
{
    /// <summary>
    ///     A representation of the fields that a user provides for registration.
    /// </summary>
    /// <remarks>
    ///    This does not include the external identifier because this comes through a JWT token.
    /// </remarks>
    public class UserCreateDataRepresentation
    {
        [DataMember(Name = "email", Order = 40, EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "name", Order = 50, EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(Name = "externalId", Order = 50, EmitDefaultValue = false)]
        [Description("External system id (eg 'auth0|xxxxxxx')")]
        public string ExternalId { get; set; }
    }
}