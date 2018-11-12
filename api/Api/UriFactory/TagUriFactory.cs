using System;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;

namespace Api.UriFactory
{
    public static class TagUriFactory
    {
 
        /// <summary>
        ///     The route name for all tags across all tenants
        /// </summary>
        public const string AllTagsRouteName = "AllTags";

        /// <summary>
        ///     The route name for tags found on todo list
        /// </summary>
        public const string TodoTagsRouteName = "TodosTags";

        /// <summary>
        ///     The route name for a create form for a uri-list media type
        /// </summary>
        /// <remarks>
        ///    This is experimental as a way to describe the uri-list
        /// </remarks>
        ///  <see cref="MediaType.UriList"/>
        public const string EditFormUriListRouteName = "TagsUriListCreateForm";

        /// <summary>
        ///     The route name for an edit form for JSON patch media type
        /// </summary>
        ///  <see cref="MediaType.JsonPatch"/>
        public const string EditFormJsonPatchRouteName = "TagsJsonPatchCreateForm";

        /// <summary>
        ///     The route name for create form for a tag 
        /// </summary>
        public const string CreateFormRouteName = "TagCreateForm";

        /// <summary>
        ///     The route name for a tag on a logical todo resource
        /// </summary>
        /// <remarks>
        ///    This could be on either a list or item (but in practice lists are not tagged)
        /// </remarks>
        public const string TodoTagRouteName = "TodoTag";

        /// <summary>
        ///     The route name for canonical form of a tag
        /// </summary>
        public const string TagRouteName = "Tag";

        /// <summary>
        ///     The route name for a todo collection available for a tag
        /// </summary>
        public const string TagTodoCollectionRouteName = "TagTodoCollection";

        /// <summary>
        ///     The url for a collection resource for the list of tags on a todo resource
        /// </summary>
        public static string MakeTodoTagCollectionUri(this string id, IUrlHelper url)
        {
            return url.Link(TodoTagsRouteName, new {id = id});
        }

        /// <summary>
        ///     The url for a tag resource in the context of a todo
        /// </summary>
        /// <remarks>
        ///    This is used for GET, DELETE, PUT, PATCH
        /// </remarks>
        public static string MakeTodoTagUri(this string id, string todoId, IUrlHelper url)
        {
            return url.Link(TodoTagRouteName, new {id = todoId, tagId = id});
        }

        /// <summary>
        ///     The url for a tag resource
        /// </summary>
        public static string MakeTagUri(this string id, IUrlHelper url)
        {
            return url.Link(TagRouteName, new {id = id});
        }

        /// <summary>
        ///     The url for a collection resource of all tags
        /// </summary>
        public static string MakeAllTagsCollectionUri(this IUrlHelper url)
        {
            return url.Link(AllTagsRouteName, new { });
        }

        /// <summary>
        ///     The url for a create form resource for a tag
        /// </summary>
        public static string MakeTagCreateFormUri(this string id, IUrlHelper url)
        {
            return url.Link(CreateFormRouteName, new {id = id});
        }

        /// <summary>
        ///     The url for a uri-list edit form resource for a tag 
        /// </summary>
        ///  <see cref="MediaType.UriList"/>
        public static string MakeTagEditFormUriListUri(this string id, IUrlHelper url)
        {
            return url.Link(EditFormUriListRouteName, new {id = id});
        }

        /// <summary>
        ///     The url for a json patch document edit form resource for a tag 
        /// </summary>
        ///  <see cref="MediaType.JsonPatch"/>
        public static string MakeTagEditFormJsonPatchUri(this string id, IUrlHelper url)
        {
            return url.Link(EditFormJsonPatchRouteName, new {id = id});
        }

        /// <summary>
        ///     The url for a collection resource for the list of todos in the context of a tag
        /// </summary>
        public static string MakeTagTodoCollectionUri(this string id, IUrlHelper url)
        {
            return url.Link(TagTodoCollectionRouteName, new { id });
        }
    }
}