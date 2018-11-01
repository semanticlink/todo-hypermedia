using System;
using System.Collections.Generic;

namespace Domain.Models
{
    /// <summary>
    ///     A <see cref="Todo"/> is implicitly one of of two types upon creation
    /// </summary>
    [Flags]
    public enum TodoType
    {
        List = 0,
        Item = 1
    }

    public class TodoCreateData
    {
        /// <summary>
        ///     Used for the recursive structure of todos to flag the parent. Top level todos use the tenant as
        ///     the parent. 
        /// </summary>
        public string Parent { get; set; }
        
        /// <summary>
        ///     Used for whether it is a list or item
        /// </summary>
        public TodoType Type { get; set; }
        
        public string Name { get; set; }
        public string State { get; set; }
        public List<string> Tags { get; set; }
        public DateTime Due { get; set; }
        
    }
}