using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class UserUriFactory
    {
        public const string SelfRouteName = "UsersCollectionRouteName";
        public const string UserRouteName = "UserRouteName";
        public const string CreateFormRouteName = "UserCreateForm";
        public const string EditFormRouteName = "UserEditForm";

        public static string MakeUserCollectoinUri(this IUrlHelper url)
        {
            return url.Link(SelfRouteName, new { });
        }

        public static string MakeUserUri(this string id, IUrlHelper url)
        {
            return url.Link(UserRouteName, new {id = id});
        }

        public static string MakeUserCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }

        public static string MakeUserEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }
    }
}