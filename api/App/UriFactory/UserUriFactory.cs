using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class UserUriFactory
    {
        public const string UserRouteName = "UserRouteName";
        public const string UserCollectionRouteName = "UserRouteName";
        public const string UserMeName = "UserMeName";
        public const string CreateFormRouteName = "UserCreateForm";
        public const string RegisterCreateFormRouteName = "RegisterCreateFormRouteName";
        public const string EditFormRouteName = "UserEditForm";
        public const string UserTodoCollectionName = "UserTodoCollectionRouteName";

        public static string MakeUserUri(this string id, IUrlHelper url)
        {
            return url.Link(UserRouteName, new {id = id});
        }

        public static string MakeUserCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }

        public static string MakeRegisterUserCreateFormUri(this string tenantId, IUrlHelper url)
        {
            return url.Link(RegisterCreateFormRouteName, new {id = tenantId});
        }

        public static string MakeUserEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }

        public static string MakeUserMeUri(this IUrlHelper url)
        {
            return url.Link(UserMeName, new { });
        }

        public static string MakeUserTodoCollectionUri(this string id, IUrlHelper url)
        {
            return url.Link(UserTodoCollectionName, new {id = id});
        }

    }
}