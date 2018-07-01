namespace Domain.Models
{
    /// <summary>
    ///     Configuration of the Auth0 service for clients (particular javascript)
    /// </summary>
    public class Auth0Configuration
    {
        public const string SectionName = "Auth0";
        
        public string Audience { get; set; }
        public string ClientId { get; set; }
        public string Domain { get; set; }


        public int Leeway { get; set; } = 30;
        public string RequestedScopes { get; set; } = "openid profile offline_access";
        public string ResponseType { get; set; } = "token id_token";
    }
}