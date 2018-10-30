using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class TodoUriFactory
    {
        public const string TodoRouteName = "TodoRouteName";
        public const string CreateFormRouteName = "TodoCreateFormRouteName";
        public const string EditFormRouteName = "TodoEditFormRouteName";

        public static string MakeTodoUri(this string todoId, IUrlHelper url)
        {
            return url.Link(TodoRouteName, new {id = todoId});
        }

        public static string MakeTodoCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }

        public static string MakeTodoEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }
    }
}