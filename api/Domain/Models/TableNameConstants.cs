using System.Collections.Generic;

namespace Domain.Models
{
    public class TableNameConstants
    {
        public const string Todo = "Todo";
        public const string Tenant = "Tenant";
        public const string User = "User";
        public const string Tag = "Tag";
        public const string UserRights = "UserRights";

        public static List<string> AllTables = new List<string>
        {
            Todo,
            Tenant,
            User,
            Tag,
            UserRights
        };
    }

    public class HashKeyConstants
    {
        public const string DEFAULT = "Id";
    }
}