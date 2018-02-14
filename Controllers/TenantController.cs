using Microsoft.AspNetCore.Mvc;
using TodoApi.Db;
using TodoApi.Representation;
using TodoApi.RepresentationExtensions;
using TodoApi.UriFactory;

namespace TodoApi.Controllers
{
    [Route("tenant/")]
    public class TenantController : Controller
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantController(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        [HttpGet("{id:long}", Name = TenantUriFactory.SelfRouteName)]
        public TenantRepresentation Get(long id)
        {
            return _tenantRepository
                .Get(id)
                .ToRepresentation(Url);
        }
    }
}