using System;
using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    [DynamoDBTable("Tenant")]
    public class User
    {
        [DynamoDBHashKey] public string Id { set; get; }
        
        /// <summary>
        ///     User ID held in the `AspNetUser` table in authentication module
        /// </summary>
        public string IdentityId { get; set; }
        
        /// <summary>
        ///     Tenant that the user belongs to
        ///     NOTE: tenant currently only belongs to one tenant
        /// </summary>
        public string tenantId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}