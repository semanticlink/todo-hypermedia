using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Domain;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using NLog;
using Toolkit;

namespace Infrastructure.NoSQL
{

    public class UserStore : IUserStore
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private readonly IDynamoDBContext _context;
        private readonly IIdGenerator _idGenerator;
        private readonly User _user;
        private readonly IUserRightStore _userRightStore;

        public UserStore(IDynamoDBContext context, IIdGenerator idGenerator, User user, IUserRightStore userRightStore)
        {
            _context = context;
            _idGenerator = idGenerator;
            _user = user;
            _userRightStore = userRightStore;
        }

        public async Task<string> Create(string identityId, UserCreateDataRepresentation data)
        {
            return await Create(_user, identityId, data);
        }

        /// <summary>
        ///     This method should only be external called when using the a trusted user through trusted code
        ///     because it overrides the injected user from the context.
        /// </summary>
        /// <remarks>
        ///    KLUDGE: this is easier than trying to reset the constructor inject of <see cref="User"/>
        /// </remarks>
        public async Task<string> Create(User creator, string identityId, UserCreateDataRepresentation data)
        {
            creator.ThrowArgumentExceptionIfNull("User doing the creating is not specified");
            data.ThrowArgumentExceptionIfNull("No email and name");
            
            (await GetByExternalId(identityId))
                .ThrowInvalidDataExceptionIfNotNull("User already created");

            // KLUDGE: both need to be injected

            var now = DateTime.UtcNow;

            var create = new User
            {
                Id = _idGenerator.New(),
                ExternalIds = new List<string> {identityId},
                Email = data.Email,
                Name = data.Name,
                CreatedBy = creator.Id,
                CreatedAt = now,
                UpdatedAt = now
            };

            Log.TraceFormat("New user {0} created by user {1}", create.Id, creator.Id);

            await _context.SaveAsync(create);

            return create.Id;
        }

        /// <summary>
        ///     This method should only be external called when using the a trusted user through trusted code
        ///     because it overrides the injected user from the context.
        /// </summary>
        /// <remarks>
        ///    KLUDGE: this is easier than trying to reset the constructor inject of <see cref="User"/>
        /// </remarks>
        public async Task<string> Create(
            string creatorId,
            string resourceId,
            string userExternalId,
            UserCreateDataRepresentation data,
            Permission callerRights,
            IDictionary<RightType, Permission> callerCollectionRights)
        {
            creatorId.ThrowArgumentExceptionIfNull("Need a user to do the creating");
            
            (await GetByExternalId(userExternalId))
                .ThrowInvalidDataExceptionIfNotNull("User already created");

            // KLUDGE: both need to be injected
            var now = DateTime.UtcNow;

            var newUser = new User
            {
                Id = _idGenerator.New(),
                ExternalIds = new List<string> {userExternalId},
                Email = data.Email,
                Name = data.Name,
                CreatedBy = creatorId,
                CreatedAt = now,
                UpdatedAt = now
            };

            Log.TraceFormat("New user {0} created by user {1}", newUser.Id, creatorId);

            await _context.SaveAsync(newUser);

            await _userRightStore.CreateRights(
                newUser.Id,
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
                    }
                });


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
            var user = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            updater(user);
            user.Id = id;
            user.UpdateBy = _user.Id;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveAsync(user);
        }

        public async Task Delete(string id)
        {
            var user = await Get(id)
                .ThrowObjectNotFoundExceptionIfNull();

            await _context.DeleteAsync(user);
        }
    }
}