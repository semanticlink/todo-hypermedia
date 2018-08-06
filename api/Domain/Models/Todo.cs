using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Domain.Representation.Enum;

namespace Domain.Models
{
    [DynamoDBTable(TableNameConstants.Todo)]
    public class Todo
    {
        [DynamoDBHashKey] public string Id { get; set; }

        /// <summary>
        ///     Todos logically belong to a user and a tenant
        /// </summary>
        /// <remarks>
        ///    Todos could actually live across tenants but for now it is only one tenant
        /// </remarks>
        public string Tenant { get; set; }

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