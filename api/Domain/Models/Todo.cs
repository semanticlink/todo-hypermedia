using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Domain.Representation.Enum;

namespace Domain.Models
{
    /// <summary>
    ///     Todo is polymorphic between a todo list and a todo item
    /// </summary>
    [DynamoDBTable(TableNameConstants.Todo)]
    public class Todo
    {
        [DynamoDBHashKey] public string Id { get; set; }

        /// <summary>
        ///     <para>Parent is a recursive structure for todos. A parent at the top level is a tenant</para>
        ///     <para>Note: At this stage, it is only implemented to one level deep</para>
        /// </summary>
        /// <remarks>
        ///    An empty parent is a top-level todo list. If the parent has a value it is a todo item
        /// </remarks>
        [DynamoDBProperty("Parent")]
        public string Parent { get; set; }

        public TodoType Type { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        /// <summary>
        ///     The state machine of todo
        /// </summary>
        /// <remarks>
        ///    It is currently hardcoded although this could be dynamic
        /// </remarks>
        /// <see cref="TodoState"/>
        public string State { get; set; }

        /// <summary>
        ///     Todos can be tagged as a way of grouping.
        /// </summary>
        /// <remarks>
        ///    This is an array of hash key IDs. See <see cref="Models.Tag"/>
        /// </remarks>
        [DynamoDBProperty("Tag")]
        public List<string> Tags { get; set; }

        public DateTime Due { get; set; }

        [DynamoDBVersion] public int? VersionNumber { get; set; }

        public string CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}