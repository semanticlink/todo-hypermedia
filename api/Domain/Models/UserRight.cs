using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    [DynamoDBTable(TableNameConstants.UserRight)]
    public class UserRight
    {
        [DynamoDBHashKey] public string Id { get; set; }
        public string ResourceId { get; set; }
        public ResourceType Type { get; set; }
        public string UserId { get; set; }
        public Permission Rights { get; set; }
    }
}