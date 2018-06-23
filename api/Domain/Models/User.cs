using System;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Identity;

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
        public string TenantId { get; set; }

        /// <summary>
        ///     The name/title the user wants to see on the screen at first instance. It is likely
        ///     that this will at first be the email address of the user.
        /// </summary>
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    ///     Todo User is a specialised form of the <see cref="IdentityUser"/>
    /// </summary>
    public class TodoUser : IdentityUser<string>
    {
        /// <summary>
        ///    Initializes a new instance of <see cref="IdentityUser" />.
        /// </summary>
        /// <remarks>
        ///    The Id property is initialized to form a new unique string value. 
        /// </remarks>
        public TodoUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="IdentityUser" />.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <remarks>
        ///    The Id property is initialized to form a new unique string value. 
        /// </remarks>
        public TodoUser(string userName)
            : this()
        {
            UserName = userName;
        }
    }
}