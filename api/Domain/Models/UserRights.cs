using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    [DynamoDBTable(TableNameConstants.UserRights)]
    public class UserRights
    {
        [DynamoDBHashKey] public string Id { get; set; }
        public string ResourceId { get; set; }
        public ResourceType Type { get; set; }
        public string UserId { get; set; }
        public Permissions Rights { get; set; }
    }
}