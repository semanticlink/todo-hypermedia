using Microsoft.AspNetCore.Mvc;

namespace Api.UriFactory
{
    public static class TodoUriFactory
    {
 
        /// <summary>
        ///     The route name for todo item resource
        /// </summary>
        /// <remarks>
        ///    Todo resources are recursive and can be either an list or an item. The API implementation means that
        ///     this is an **item**
        /// </remarks>
        public const string TodoRouteName = "Todo";

        /// <summary>
        ///     The route name for a create form for a todo resource 
        /// </summary>
        public const string CreateFormRouteName = "TodoCreateForm";

        /// <summary>
        ///     The route name for an edit form a todo resource
        /// </summary>
        public const string EditFormRouteName = "TodoEditForm";

        /// <summary>
        ///     The route name for a todo list resource
        /// </summary>
        /// <remarks>
        ///    Todo resources are recursive and can be either an list or an item. The API implementation means that
        ///     this is a **list**
        /// </remarks>
        public const string TodoTodoListRouteName = "TodoListTodos";


        /// <summary>
        ///     The url for a todo resource 
        /// </summary>
        public static string MakeTodoUri(this string todoId, IUrlHelper url)
        {
            return url.Link(TodoRouteName, new {id = todoId});
        }

        /// <summary>
        ///     The url for a create form for a todo resource 
        /// </summary>
        public static string MakeTodoCreateFormUri(this IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new { });
        }

        /// <summary>
        ///     The url for an edit form for a todo resource 
        /// </summary>
        public static string MakeTodoEditFormUri(this IUrlHelper url)
        {
            return url.Link(EditFormRouteName, new { });
        }

        /// <summary>
        ///     The url for a todo list resource 
        /// </summary>
        public static string MakeTodoTodoListUri(this string id, IUrlHelper url)
        {
            return url.Link(TodoTodoListRouteName, new {id = id});
        }
    }
}