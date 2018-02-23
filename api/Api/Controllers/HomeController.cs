using System;
using System.Linq;
using Api.Web;
using App.RepresentationExtensions;
using App.UriFactory;
using Domain.Models;
using Domain.Persistence;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly Version _version;
        private readonly User _user;
        private readonly ITenantStore _tenantStore;
        private readonly ITodoStore _todoStore;

        public HomeController(Version version, User user, ITenantStore tenantStore, ITodoStore todoStore)
        {
            _version = version;
            _user = user;
            _tenantStore = tenantStore;
            _todoStore = todoStore;
        }

        [HttpGet("", Name = HomeUriFactory.SelfRouteName)]
        public ApiRepresentation GetApi()
        {
            var apiVersion = new ApiVersion
            {
                Version = _version.ToString()
            };

            return apiVersion
                .ToRepresentation(Url);
        }

        ///////////////////////////////////////////////////////////////
        //
        //  The collection of tenant resource
        //  =================================

        /// <summary>
        ///     Provides a redirect URL for locating a tenant without disclosing the name of any other tenants.
        /// </summary>
        [HttpGet(@"a/{tenantCode:regex(^[[\w\d\-\.]]+$)}")]
        public IActionResult GetTenant(string tenantCode)
        {
            var tenant = _tenantStore
                .GetByCode(tenantCode)
                .Result
                .ThrowObjectNotFoundExceptionIfNull("Invalid tenant");
            return new RedirectResult(tenant.Id.MakeTenantUri(Url), permanent: false);
        }

        ///////////////////////////////////////////////////////////////
        //
        //  The collection of tenant resource
        //  =================================
        //

        /// <summary>
        ///     This is a logical resource which represents all tenants or a search for a single tenant (as a list)
        /// </summary>
        /// <remarks>
        ///     If the user is an administrator we could disclose the list of
        ///     all tenants. However for normal users we could disclose their
        ///     single tenant in the collection. For anonymous user the list **must**
        ///     be empty.
        /// </remarks>
        [HttpGet("tenant/", Name = HomeUriFactory.TenantsRouteName)]
        public FeedRepresentation GetTenants([FromQuery(Name = "q")] string search = null)
        {
            return (!string.IsNullOrWhiteSpace(search)
                    //
                    //  Regardless of whether the caller is authenticated or not, a query with a name
                    //  will return a collection with zero or one items matched by tenant code.
                    //
                    ? _tenantStore.GetByCode(search).Result.ToEnumerable()
                    //
                    : _user != null
                        // If the user is authenticated, then return all tenants that the user has access to.
                        ? _tenantStore.GetTenantsForUser(_user.Id).Result

                        // The user is not authenticated and there is no query, so the caller gets no tenants.
                        : new Tenant[] { })
                .ToRepresentation(Url);
        }


        /// <summary>
        ///     Perform a search for a tenant. This is a highly constrainted that will only disclose a single tenant
        ///     if the caller knows its code.
        /// </summary>
        [HttpPost("tenant/search/", Name = HomeUriFactory.HomeTenantSearchRouteName)]
        public IActionResult Search([FromBody] TeantSearchRepresentation criteria)
        {
            return criteria
                .ThrowInvalidDataExceptionIfNull("Invalid search form")
                .Search
                .ThrowInvalidDataExceptionIfNullOrWhiteSpace("Invalid tenant search name")
                .MakeHomeTenantsUri(Url)
                .MakeCreated(Request, "Search resource created");
        }

        /// <summary>
        ///     A simple search form resource.
        /// </summary>
        [HttpGet("tenant/form/search", Name = HomeUriFactory.HomeTenantSearchFormRouteName)]
        public SearchFormRepresentation GetTenantsSearchForm()
        {
            return new TenantRepresentation()
                .ToTenantSearchFormRepresentation(Url);
        }
    }
}