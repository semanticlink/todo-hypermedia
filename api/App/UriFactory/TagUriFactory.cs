using Domain.Models;
using Microsoft.AspNetCore.Mvc;

public static class TagUriFactory
{
    public const string AllTagsRouteName = "AllTagsRouteName";
    public const string TodoTagsRouteName = "TodosTagsRouteName";
    public const string CreateFormRouteName = "TagsCreateRouteName";
    public const string TodoTagRouteName = "TodoTagRouteName";
    public const string TagRouteName = "TagRouteName";

    public const string TodoTagCreateRouteName = "TodoTagCreateRouteName";

    public static string MakeTodoTagCollectionUri(this string id, IUrlHelper url)
    {
        return url.Link(TodoTagsRouteName, new {id = id});
    }

    public static string MakeTodoTagUri(this string id, string todoId, IUrlHelper url)
    {
        return url.Link(TodoTagRouteName, new {id = todoId, tagId = id});
    }

    public static string MakeTagUri(this string id, IUrlHelper url)
    {
        return url.Link(TagRouteName, new {id = id});
    }

    public static string MakeAllTagsCollectionUri(this IUrlHelper url)
    {
        return url.Link(AllTagsRouteName, new { });
    }

    public static string MakeTagCreateFormUri(this string id, IUrlHelper url)
    {
        return url.Link(CreateFormRouteName, new {id = id});
    }

    public static string MakeCreateTagUri(this IUrlHelper url)
    {
        return url.Link(TodoTagRouteName, new { });
    }
}