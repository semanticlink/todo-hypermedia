using Api.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;

namespace Api.RepresentationExtensions
{
    public static class ApiRepresentationExtensions
    {
        public static ApiRepresentation ToRepresentation(this ApiVersion api, IUrlHelper url)
        {
            return new ApiRepresentation
            {
                Links = new[]
                {
                    // root of the api
                    url.MakeHomeUri().MakeWebLink(IanaLinkRelation.Self),
                                        
                    // a virtual resource that redirects to the user
                    url.MakeUserMeUri().MakeWebLink(CustomLinkRelation.Me),
                    
                    // all authentication approaches
                    url.MakeAuthenicateUri().MakeWebLink(CustomLinkRelation.Authenticate),
                    
                    // all tags currently created across todos
                    url.MakeAllTagsCollectionUri().MakeWebLink(CustomLinkRelation.Tags),

                    //  tenant collection which has the tenant search on it
/*
                    url.MakeHomeTenantsUri().MakeWebLink(CustomLinkRelation.Tenants),
*/

                    //  user collection which has the tenant search on it
                    url.MakeHomeUsersUri().MakeWebLink(CustomLinkRelation.Users),
                },
                Version = api.Version
            };
        }
    }
}