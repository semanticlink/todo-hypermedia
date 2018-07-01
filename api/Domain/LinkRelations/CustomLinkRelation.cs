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
               
        public const string Authenticate = "authenticate";
        
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