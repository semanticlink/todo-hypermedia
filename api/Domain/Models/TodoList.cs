using System;
using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    [DynamoDBTable(TableNameConstants.TodoList)]
    public class TodoList
    {
        [DynamoDBHashKey] public string Id { get; set; }
        
        /// <summary>
        ///     A list name belongs to a tenant
        /// </summary>
        public string Tenant { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        [DynamoDBVersion] public int? VersionNumber { get; set; }

        public string CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}