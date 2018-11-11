namespace SemanticLink.AspNetCore
{
    public static class AuthenticatorDefaults
    {
        /// <summary>
        ///<para>
        ///     Authentication sscheme for against external integrations such as Auth0 that return a JSON Web Token (JWT).
        ///     see https://tools.ietf.org/html/rfc7519
        /// </para>
        /// <example>
        ///    wwww-authentication: jwt realm="api-auth0", uri=https://example.com/authenticate
        /// </example>
        /// </summary>
        /// <remarks>
        ///    Don't confuse JWT with Java Web Tokens
        /// </remarks>
        public const string ExternalAuthenticationSchemeName = "jwt";

        /// <summary>
        /// <para>
        ///    The name of the realm we are using for our jwt authentication. It is logically on the 'api' and then
        ///     suffixed by the authenticator scheme
        /// </para>
        /// <example>
        ///    realm="api-auth0"
        /// </example>
        /// </summary>
        public const string AuthenticatorAuth0Realm = "api-auth0";
        
    }
}