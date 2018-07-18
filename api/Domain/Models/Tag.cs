using System;
using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    /// <summary>
    ///     An immutable collection of tags that can be used across <see cref="Todo"/>s
    /// </summary>
    [DynamoDBTable(TableNameConstants.Tag)]
    public class Tag
    {
        [DynamoDBHashKey] public string Id { get; set; }

        public string Name { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}