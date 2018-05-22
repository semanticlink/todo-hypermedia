using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class UserUriFactory
    {
        public const string SelfRouteName = "User";
        public const string CreateFormRouteName = "UserCreateForm";
        public const string EditFormRouteName = "UserEditForm";

        public static string MakeUserUri(this string userId, IUrlHelper url)
        {
            return url.Link(SelfRouteName, new {id = userId});
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