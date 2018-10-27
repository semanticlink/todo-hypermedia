using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class TodoListUriFactory
    {
        public const string SelfRouteName = "TodoListRouteName";
        public const string TodoListRouteName = "TodoListRouteName";
        public const string TodoListTodosRouteName = "TodoListTodosRouteName";
        public const string CreateFormRouteName = "TodoListCreateFormRouteName";
        public const string EditFormRouteName = "TodoListEditFormRouteName";

        public static string MakeTodoListUri(this string id, IUrlHelper url)
        {
            return url.Link(SelfRouteName, new {id = id});
        }

        public static string MakeTodoListCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }

        public static string MakeTodoListEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }

        public static string MakeTodoListTodosUri(this string id, IUrlHelper url)
        {
            return url.Link(TodoListTodosRouteName, new {id = id});
        }
    }
}