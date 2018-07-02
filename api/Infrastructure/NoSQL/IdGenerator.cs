using System;

namespace Infrastructure.NoSQL
{
    public static class IdGenerator
    {
        /// <summary>
        ///     Poor strategy to generate IDs for records
        /// </summary>
        /// <returns></returns>
        public static string New()
        {
            var s = Guid.NewGuid().ToString();
            return s.Substring(s.Length - 10); 
        }
    }
}