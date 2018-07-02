using System.Collections.Generic;

namespace Infrastructure.NoSQL
{
    public class TableNameConstants
    {
        public const string Todo = "Todo";
        public const string Tenant = "Tenant";
        public const string User = "User";
        public const string Tag = "Tag";
        
        public static List<string> AllTables = new List<string>
        {
            Todo,
            Tenant,
            User,
            Tag
        };
    }

    public class HashKeyConstants
    {
        public const string DEFAULT = "Id";
    }
}