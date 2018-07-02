namespace Domain.LinkRelations
{
    public static class CustomLinkRelation
    {
        ///************************************
        ///
        ///   Form links
        ///
        ///************************************
        public const string Search = "search";

        public const string Submit = "submit";


        ///************************************
        ///
        ///   Authentication/context links
        ///
        ///************************************
        
        ///<summary>
        ///    Collection of different authenticate strategies available
        /// </summary>
        public const string Authenticate = "authenticate";
        public const string Authenticator = "authenticator";

        /// <summary>
        ///     Link to the configuration of the Auth0 service
        /// </summary>
        public const string Auth0 = "auth0";

        /// <summary>
        ///     Link to the collection avaiallable to the authenticated user
        /// </summary>
        public const string Me = "me";

        /// <summary>
        ///     Tenants collection for authenticated users
        /// </summary>
        public const string Tenants = "tenants";

        /// <summary>
        ///     Users collection on a tenant
        /// </summary>
        public const string Users = "users";


        ///************************************
        ///
        ///   DOMAIN links
        ///
        ///************************************
        /// <summary>
        ///     Todo collection for a user/tenant
        /// </summary>
        public const string Todos = "todos";

        /// <summary>
        ///    Tags (categories) available on a todo
        /// </summary>
        public const string Tags = "tags";
    }
}