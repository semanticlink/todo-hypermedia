using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    [DynamoDBTable(TableNameConstants.UserInheritRight)]
    public class UserInheritRight
    {
        [DynamoDBHashKey] public string Id { get; set; }
        public string ResourceId { get; set; }
        public ResourceType Type { get; set; }
        public string UserId { get; set; }
        public Permission Rights { get; set; }
        
        /// <summary>
        ///     Matches the <see cref="UserRight.Type"/>
        /// </summary>
        public ResourceType InheritType { get; set; }
    }
}