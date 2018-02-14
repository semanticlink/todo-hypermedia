using Microsoft.AspNetCore.Mvc;
using TodoApi.Controllers;
using TodoApi.LinkRelations;
using TodoApi.Models;
using TodoApi.Representation;
using TodoApi.Representation.LinkedRepresentation;
using TodoApi.UriFactory;

namespace TodoApi.RepresentationExtensions
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
                    url.MakeApiUri().MakeWebLink(IanaLinkRelation.Self),
                    
                    // todos collection
                    url.MakeTodoCollectionUri().MakeWebLink(CustomLinkRelation.Todos)
                },
                Version = api.Version
            };
        }
    }
}