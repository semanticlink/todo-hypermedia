using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using NLog;
using Toolkit;

namespace Infrastructure.NoSQL
{
    public class UserStore : IUserStore
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private readonly IDynamoDBContext _context;
        private readonly User _creator;
        private readonly IIdGenerator _idGenerator;
        private readonly IUserRightStore _userRightStore;

        public UserStore(
            User creator,
            IDynamoDBContext context,
            IIdGenerator idGenerator,
            IUserRightStore userRightStore)
        {
            _creator = creator;
            _context = context;
            _idGenerator = idGenerator;
            _userRightStore = userRightStore;
        }

        /// <summary>
        ///     <para>
        ///         This method should only be external called when using the a trusted user through trusted code
        ///         because it overrides the injected user from the context.
        ///     </para>
        ///     <para>
        ///         If the user already exists, it just return that <see cref="User.Id" />.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     KLUDGE: this is easier than trying to reset the constructor inject of <see cref="User" />
        /// </remarks>
        /// <returns>Id of the new or existing user</returns>
        public async Task<string> Create(
            string ownerId,
            string resourceId,
            UserCreateData data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights)
        {
            if (await GetByExternalId(data.ExternalId) is User existingUser) return existingUser.Id;

            // KLUDGE: needs to be injected
            var now = DateTime.UtcNow;

            var newUser = new User
            {
                Id = _idGenerator.New(),
                ExternalIds = new List<string> {data.ExternalId},
                Email = data.Email,
                Name = data.Name,
                CreatedBy = _creator.Id,
                CreatedAt = now,
                UpdatedAt = now
            };

            Log.TraceFormat("New user {0} '{1}' created by user {2}", newUser.Id, newUser.Email, _creator.Id);

            await _context.SaveAsync(newUser);

            // create rights for new user and also the owner (if the owner if different)
            await Task.WhenAll(
                new List<string> {newUser.Id, ownerId}
                    .Where(x => !x.IsNullOrWhitespace())
                    .Distinct()
                    .Select(id => _userRightStore.CreateRights(
                        id,
                        newUser.Id,
                        RightType.User.MakeCreateRights(callerRights, callerCollectionRights),
                        new InheritForm
                        {
                            Type = RightType.RootUserCollection,
                            ResourceId = resourceId,
                            InheritedTypes = new List<RightType>
                            {
                                RightType.User,
                                RightType.UserTenantCollection,
                                RightType.UserTodoCollection
                            },
                            CopyInheritTypes = new List<RightType>
                            {
                                RightType.Todo,
                                RightType.TodoTagCollection,
                                RightType.TodoCommentCollection,
                                //
                                RightType.Tag,
                                //
                                RightType.Comment
                            }
                        })));

            return newUser.Id;
        }

        public async Task<User> Get(string id)
        {
            return await _context.SingleOrDefault<User>(id);
        }

        public async Task<User> GetByExternalId(string externalId)
        {
            externalId
                .ThrowInvalidDataExceptionIfNullOrWhiteSpace("External Id must be set");

            return await _context.FirstOrDefault<User>(
                new ScanCondition(nameof(User.ExternalIds), ScanOperator.Contains, externalId));
        }

        public async Task<bool> IsRegistered(string id)
        {
            return (await Get(id)).IsNotNull();
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _context.Where<User>();
        }

        public async Task Update(string id, Action<User> updater)
        {
            var user = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            updater(user);
            user.Id = id;
            user.UpdateBy = _creator.Id;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveAsync(user);
        }

        public async Task Delete(string id)
        {
            var user = (await Get(id))
                .ThrowObjectNotFoundExceptionIfNull();

            // TODO: delete all rights

            await _context.DeleteAsync(user);
        }
    }
}