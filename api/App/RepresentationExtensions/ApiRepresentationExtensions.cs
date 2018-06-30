using App.UriFactory;
using Domain.LinkRelations;
using Domain.Models;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using Toolkit.LinkRelations;
using Toolkit.Representation.LinkedRepresentation;

namespace App.RepresentationExtensions
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
                    
                    // authentication resource that is point to in the 401 response
                    url.MakeAuthenticatePasswordUri().MakeWebLink(CustomLinkRelation.Authenticate),
                    
                    // a virtual resource that redirects to the user
                    url.MakeUserMeUri().MakeWebLink(CustomLinkRelation.Me),
                    
                    // all tags currently created across todos
                    url.MakeAllTagsCollectionUri().MakeWebLink(CustomLinkRelation.Tags),

                    //  tenant collection which has the tenant search on it
                    url.MakeHomeTenantsUri().MakeWebLink(CustomLinkRelation.Tenants),
                },
                Version = api.Version
            };
        }
    }
}