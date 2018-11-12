using Microsoft.AspNetCore.Mvc;

namespace Api.UriFactory
{
    public static class UserUriFactory
    {
        /// <summary>
        ///     The route name for 
        /// </summary>
        public const string UserRouteName = "User";

        /// <summary>
        ///     The route name for 
        /// </summary>
        public const string UserMeName = "UserMeName";

        /// <summary>
        ///     The route name for 
        /// </summary>
        public const string CreateFormRouteName = "UserCreateForm";

        /// <summary>
        ///     The route name for 
        /// </summary>
        public const string EditFormRouteName = "UserEditForm";

        /// <summary>
        ///     The route name for 
        /// </summary>
        public const string UserTodosRouteName = "UserTodoList";

        /// <summary>
        ///     The route name for 
        /// </summary>
        public const string UserTenantsRouteName = "UserTenants";

        /// <summary>
        ///     The route name for 
        /// </summary>
        public const string UserTenantRouteName = "UserTenant";

        /// <summary>
        ///     The route name for 
        /// </summary>
        public const string UserTenantTodoRouteName = "UserTenantTodoList";

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        public static string MakeUserUri(this string id, IUrlHelper url)
        {
            return url.Link(UserRouteName, new {id = id});
        }

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        public static string MakeUserCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        public static string MakeUserEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        public static string MakeUserMeUri(this IUrlHelper url)
        {
            return url.Link(UserMeName, new { });
        }

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        public static string MakeUserTodosUri(this string id, IUrlHelper url)
        {
            return url.Link(UserTodosRouteName, new {id = id});
        }

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        public static string MakeUserTenantsUri(this string id, IUrlHelper url)
        {
            return url.Link(UserTenantsRouteName, new {id = id});
        }

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        public static string MakeUserTenantUri(this string id, string tenantId, IUrlHelper url)
        {
            return url.Link(UserTenantRouteName, new {id = id, tenantId = tenantId});
        }

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        /// <remarks>
        ///     A wrapper to reverse having a tenantId before a userid
        /// </remarks>
        /// <see cref="MakeUserTenantUri"/>
        public static string MakeTenantForUserUri(this string tenantId, string id, IUrlHelper url)
        {
            return id.MakeUserTenantUri(tenantId, url);
        }

        /// <summary>
        ///     The url for a collection resource for the list of 
        /// </summary>
        public static string MakeUserTenantTodoListUri(this string id, string tenantId, IUrlHelper url)
        {
            return url.Link(UserTenantTodoRouteName, new {id = id, tenantId = tenantId});
        }
    }
}