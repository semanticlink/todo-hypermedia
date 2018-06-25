using System.Threading.Tasks;
using Api.Web;
using App;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;
using Marvin.Cache.Headers;

namespace Api.Controllers
{
    [Route("user")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITodoStore _todoStore;

        public UserController(
            UserManager<IdentityUser> userManager,
            ITodoStore todoStore)
        {
            _userManager = userManager;
            _todoStore = todoStore;
        }

        [HttpGet("me", Name = UserUriFactory.UserMeName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public IActionResult Me()
        {
            return User.ToUser()
                .Id
                .MakeUserUri(Url)
                .MakeRedirect();
        }

        [HttpGet("{id}", Name = UserUriFactory.UserRouteName)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Private)]
        [HttpCacheValidation(AddNoCache = true)]
        public async Task<UserRepresentation> Get(string id)
        {
            return (await _userManager.FindByIdAsync(id))
                .ThrowInvalidDataExceptionIfNull($"User '{id}' not found")
                .ToRepresentation(id, Url);
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

        /////////////////////////
        //
        // Todo collection on a user

        [HttpGet("{id}/todo", Name = UserUriFactory.UserTodoCollectionName)]
        public async Task<FeedRepresentation> GetUserTodos(string id)
        {
            return (await _todoStore
                    .GetAll())
                .ToFeedRepresentation(id, Url);
        }
    }
}