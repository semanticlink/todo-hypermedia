using System;
using System.ComponentModel;
using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    [DynamoDBTable("Todo")]
    public class Todo
    {
        [DynamoDBHashKey] public string Id { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }
        public bool Completed { get; set; }
        public DateTime Due { get; set; }

        [DynamoDBVersion]
        public int? VersionNumber { get; set; }
        
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        
    }
}