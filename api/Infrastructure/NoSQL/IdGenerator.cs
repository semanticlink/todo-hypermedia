using System;
using Domain;

namespace Infrastructure.NoSQL
{
    public class IdGenerator : IIdGenerator
    {
        /// <summary>
        ///     Poor strategy to generate IDs for records
        /// </summary>
        public string New()
        {
            var s = Guid.NewGuid().ToString();
            return s.Substring(s.Length - 10);
        }
    }
}