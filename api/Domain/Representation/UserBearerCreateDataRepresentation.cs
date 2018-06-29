using System.Runtime.Serialization;

namespace Domain.Representation
{
    public class UserBearerCreateDataRepresentation
    {
        [DataMember(Name = "accessToken", Order = 40, EmitDefaultValue = false)]
        public string AccessToken { get; set; }

        [DataMember(Name = "tokenType", Order = 50, EmitDefaultValue = false)]
        public string TokenType { get; set; }
    }
}