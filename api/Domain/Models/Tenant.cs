using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models
{
    [DynamoDBTable("Tenant")]
    public class Tenant
    {
        [DynamoDBHashKey] public string Id { get; set; }
        
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        /// <summary>
        ///     All user ids with access to this tenant. The current implementation uses the <see cref="UserManager{TUser}"/>
        ///     for the provision of users.
        /// </summary>
        public List<string> User { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Tenants are updateable, including when users are added/removed.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}