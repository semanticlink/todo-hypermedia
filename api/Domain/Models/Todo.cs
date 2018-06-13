using System;
using Amazon.DynamoDBv2.DataModel;
using Domain.Representation.Enum;

namespace Domain.Models
{
    [DynamoDBTable("Todo")]
    public class Todo
    {
        [DynamoDBHashKey] public string Id { get; set; }

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


        public DateTime Due { get; set; }

        [DynamoDBVersion] public int? VersionNumber { get; set; }

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}