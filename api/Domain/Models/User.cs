using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models
{
    [DynamoDBTable(TableNameConstants.User)]
    public class User
    {
        [DynamoDBHashKey] public string Id { set; get; }

        /// <summary>
        ///     User IDs held external systems (eg Auth0)
        /// </summary>
        public List<string> ExternalIds { get; set; }

        /// <summary>
        ///     The email address of the user registered in external systems
        /// </summary>
        public string Email { get; set; }
        
        /// <summary>
        ///     The name/title the user wants to see on the screen at first instance. It is likely
        ///     that this will at first be the email address of the user.
        /// </summary>
        public string Name { get; set; }
        

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}