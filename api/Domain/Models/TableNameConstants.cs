using System.Collections.Generic;

namespace Domain.Models
{
    public static class TableNameConstants
    {
        public const string TodoList = "TodoList";
        public const string Todo = "Todo";
        public const string Tenant = "Tenant";
        public const string User = "User";
        public const string Tag = "Tag";
        public const string UserRight = "UserRight";
        public const string UserInheritRight = "UserInheritRight";

        public static readonly List<string> AllTables = new List<string>
        {
            TodoList,
            Todo,
            Tenant,
            User,
            Tag,
            UserRight,
            UserInheritRight
        };
    }

    public static class HashKeyConstants
    {
        public const string Default = "Id";
    }
}