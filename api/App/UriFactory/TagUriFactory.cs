using Domain.Models;
using Microsoft.AspNetCore.Mvc;

public static class TagUriFactory
{
    public const string AllTagsRouteName = "AllTagsRouteName";
    public const string TodoTagsRouteName = "TodosTagsRouteName";
    public const string CreateFormUriListRouteName = "TagsCreateUriListRouteName";
    public const string EditFormJsonPatchRouteName = "EditFormJsonPatchRouteName";
    public const string CreateFormRouteName = "TagsCreateRouteName";
    public const string TodoTagRouteName = "TodoTagRouteName";
    public const string TagRouteName = "TagRouteName";
    public const string TagTodoCollectionRouteName = "TagTodoCollectionRouteName";

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

    public static string MakeTagEditFormUriListUri(this string id, IUrlHelper url)
    {
        return url.Link(CreateFormUriListRouteName, new {id = id});
    }

    public static string MakeTagEditFormJsonPatchUri(this string id, IUrlHelper url)
    {
        return url.Link(EditFormJsonPatchRouteName, new {id = id});
    }

    public static string MakeCreateTagUri(this IUrlHelper url)
    {
        return url.Link(TodoTagRouteName, new { });
    }

    public static string MakeTagTodoCollectionUri(this string id, IUrlHelper url)
    {
        return url.Link(TagTodoCollectionRouteName, new { id });
    }
}