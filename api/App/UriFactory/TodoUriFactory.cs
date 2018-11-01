using Microsoft.AspNetCore.Mvc;

namespace App.UriFactory
{
    public static class TodoUriFactory
    {
        public const string TodoRouteName = "TodoRouteName";
        public const string CreateFormRouteName = "TodoCreateFormRouteName";
        public const string EditFormRouteName = "TodoEditFormRouteName";
        public const string TodoTodoListRouteName = "TodoListTodosRouteName";


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

        public static string MakeTodoTodoListUri(this string id, IUrlHelper url)
        {
            return url.Link(TodoTodoListRouteName, new {id = id});
        }
    }
}