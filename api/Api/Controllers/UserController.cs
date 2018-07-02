using System.Threading.Tasks;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;
using Marvin.Cache.Headers;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [Route("user")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserStore _userStore;
        private readonly ITodoStore _todoStore;
        private readonly IConfiguration _configuration;

        public UserController(
            IUserStore userStore,
            ITodoStore todoStore,
            IConfiguration configuration)
        {
            _userStore = userStore;
            _todoStore = todoStore;
            _configuration = configuration;
        }

        [HttpGet("me", Name = UserUriFactory.UserMeName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<IActionResult> Me()
        {
            return (await _userStore.GetByExternalId(User.GetExternalId()))
                .ThrowObjectNotFoundExceptionIfNull("User does not exist")
                .Id
                .MakeUserUri(Url)
                .MakeRedirect();
        }

        [HttpGet("{id}", Name = UserUriFactory.UserRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<UserRepresentation> Get(string id)
        {
            var domain = _configuration.GetSection(Auth0Configuration.SectionName).Get<Auth0Configuration>().Domain;
            return (await _userStore.Get(id))
                .ThrowObjectNotFoundExceptionIfNull($"User '{id}' not found")
                .ToRepresentation(Url);
        }


        /// <summary>
        ///     A public stateless edit form that is fully cacheable.
        /// </summary>
        [HttpGet("form/edit", Name = UserUriFactory.EditFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        public FormRepresentation GetEditForm()
        {
            return Url.ToUserEditFormRepresentation();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromBody] UserEditData model, string id)
        {
            await _userStore.Update(id, user =>
            {
                user.Email = model.Email;
                user.Name = model.Name;
                // TODO user.ExternalIds
            });
            return NoContent();
        }

        /////////////////////////
        //
        // Todo collection on a user

        /// <summary>
        ///     User todo collection
        /// </summary>
        /// <see cref="TodoController.GetById"/>
        [HttpGet("{id}/todo", Name = UserUriFactory.UserTodoCollectionName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<FeedRepresentation> GetUserTodos(string id)
        {
            return (await _todoStore
                    .GetAll())
                .ToFeedRepresentation(id, Url);
        }
    }
}