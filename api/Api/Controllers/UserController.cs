using System.Threading.Tasks;
using Api.Authorisation;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;
using Marvin.Cache.Headers;

namespace Api.Controllers
{
    [Route("user")]
    public class UserController : Controller
    {
        private readonly IUserStore _userStore;
        private readonly ITodoStore _todoStore;

        public UserController(IUserStore userStore, ITodoStore todoStore)
        {
            _userStore = userStore;
            _todoStore = todoStore;
        }

        [HttpGet("me", Name = UserUriFactory.UserMeName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        [AuthoriseRedirect]
        public async Task<IActionResult> Me()
        {
            return User
                .GetIdentityId()
                .MakeUserUri(Url)
                .MakeRedirect();
        }

        [HttpGet("{id}", Name = UserUriFactory.UserRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        [AuthoriseUser(Permission.Get)]
        public async Task<UserRepresentation> Get(string id)
        {
            return (await _userStore
                    .Get(User.GetIdentityId()))
                .ThrowObjectNotFoundExceptionIfNull($"User '{id}' not found")
                .ToRepresentation(Url);
        }


        /// <summary>
        ///     A public stateless edit form that is fully cacheable.
        /// </summary>
        [HttpGet("form/edit", Name = UserUriFactory.EditFormRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = CacheDuration.Long)]
        [AuthoriseForm]
        public FormRepresentation GetEditForm()
        {
            return Url.ToUserEditFormRepresentation();
        }

        [HttpPut("{id}")]
        [AuthoriseUser(Permission.Put)]
        public async Task<IActionResult> UpdateUser([FromBody] UserEditData model, string id)
        {
            await _userStore.Update(id, user =>
            {
                user.Email = model.Email;
                user.Name = model.Name;
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
        [AuthoriseUserTodoCollection(Permission.Get)]
        public async Task<FeedRepresentation> GetUserTodos(string id)
        {
            return (await _todoStore
                    .GetAll())
                .ToFeedRepresentation(id, Url);
        }
    }
}