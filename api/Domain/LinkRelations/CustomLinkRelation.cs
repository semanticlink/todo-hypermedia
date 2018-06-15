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
        ///     Tenants collection for authenticated users
        /// </summary>
        public const string Tenants = "tenants";
        

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