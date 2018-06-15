using System;
using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    [DynamoDBTable("Todo")]
    public class Tag
    {
        [DynamoDBHashKey] public string Id { get; set; }


        public string Name { get; set; }

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}