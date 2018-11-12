using Api.UriFactory;
using Domain.LinkRelations;
using Domain.Representation;
using Microsoft.AspNetCore.Mvc;
using SemanticLink;
using SemanticLink.Form;

namespace Api.RepresentationExtensions
{
    public static class TagFormRepresentationExtensions
    {
        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource <see cref="TagRepresentation"/>
        /// </summary>
        /// <seealso cref = "TagCreateDataRepresentation" />
        public static CreateFormRepresentation ToTagCreateFormRepresentation(this string id, IUrlHelper url)
        {
            return new CreateFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    id.MakeTagCreateFormUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // submit against the parent collection
                    id.MakeTodoTagCollectionUri(url).MakeWebLink(CustomLinkRelation.Submit),
                },
                Items = MakeCreateFormItems()
            };
        }


        private static FormItemRepresentation[] MakeCreateFormItems()
        {
            return new FormItemRepresentation[]
            {
                new TextInputFormItemRepresentation
                {
                    Name = "name",
                    Description = "Tag (or category) name",
                    Required = true
                }
            };
        }

        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource as text/uri-list
        /// </summary>
        /// <seealso cref = "TagCreateDataRepresentation" />
        public static EditFormRepresentation ToTagEditFormUriRepresentation(this string id, IUrlHelper url)
        {
            return new EditFormRepresentation
            {
                Links = new[]
                {
                    // this collection
                    id.MakeTagCreateFormUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // submit against the parent collection
                    id.MakeTodoTagCollectionUri(url).MakeWebLink(CustomLinkRelation.Submit, type: MediaType.UriList),
                },
                Items = MakeCreateFormUriListItems()
            };
        }


        private static FormItemRepresentation[] MakeCreateFormUriListItems()
        {
            return new FormItemRepresentation[]
            {
                new GroupFormItemRepresentation
                {
                    Description =
                        "Uri list of an existing tags from the global collection to be aded. Note it can also " +
                        "include comment lines '#'",
                    Items = new FormItemRepresentation[]
                    {
                        new UriInputFormItemRepresentation()
                        {
                            Description = "A valid uri of a tag resource",
                            Multiple = true,
                            Required = false
                        },
                        new TextInputFormItemRepresentation()
                        {
                            Description = "Comments (must start the line with a '#')",
                            Multiple = true,
                            Required = false
                        },
                    }
                }
            };
        }

        /// <summary>
        ///     Get the create form to describe to clients of the API how to
        ///     modify instances on the resource as application/json-patch+json
        /// </summary>
        /// <seealso cref = "TagCreateDataRepresentation" />
        public static EditFormRepresentation ToTagEditFormJsonPatchUri(this string id, IUrlHelper url)
        {
            return new EditFormRepresentation()
            {
                Links = new[]
                {
                    // this collection
                    id.MakeTagCreateFormUri(url).MakeWebLink(IanaLinkRelation.Self),

                    // submit against the parent collection
                    id.MakeTodoTagCollectionUri(url).MakeWebLink(CustomLinkRelation.Submit, type: MediaType.JsonPatch),
                },
                Items = MakeEditFormJsonPatchItems()
            };
        }


        /// <summary>
        ///     All descriptions from https://sookocheff.com/post/api/understanding-json-patch/
        /// </summary>
        private static FormItemRepresentation[] MakeEditFormJsonPatchItems()
        {
            return new FormItemRepresentation[]
            {
                new GroupFormItemRepresentation
                {
                    Description =
                        "The typical update cycle for an API resource is to (1) GET the representation, (2) modify it " +
                        "and (3) PUT back the entire representation. This can waste bandwidth and processing time for " +
                        "large resources. An alternative is to use the HTTP PATCH extension method to only send the " +
                        "differences between two resources. HTTP PATCH applies a set of changes to the document " +
                        "referenced by the HTTP request. A JSON Patch document is a sequential list of operations " +
                        "to be applied to an object.",
                    Multiple = true,
                    Required = false,
                    Items = new FormItemRepresentation[]
                    {
                        new SelectFormItemRepresentation
                        {
                            Name = "op",
                            Multiple = false,
                            Description =
                                "Each operation is a JSON object having exactly one op member. Valid operations " +
                                "are add, remove, replace, move, copy and test.",
                            Required = true,
                            Items = new SelectOptionItemRepresentation[]
                            {
                                new SelectOptionValueItemRepresentation
                                {
                                    Type = FormType.Enum,
                                    Description =
                                        "The add operation is used in different ways depending on the target of " +
                                        "the path being referenced. Generally speaking we can use add to append to " +
                                        "a list, add a member to an object or update the value of an existing field. " +
                                        "The add operation accepts a value member which is the value to update " +
                                        "the referenced path.",
                                    Label = "Add",
                                    Value = "add",
                                    Name = "add",
                                },
                                new SelectOptionValueItemRepresentation
                                {
                                    Type = FormType.Enum,
                                    Description =
                                        "Remove is a simple operation. The target location of the path is removed " +
                                        "from the object.",
                                    Label = "Remove",
                                    Value = "remove",
                                    Name = "remove",
                                },
                                new SelectOptionValueItemRepresentation
                                {
                                    Type = FormType.Enum,
                                    Description =
                                        "Replace is used to set a new value to a member of the object. It is " +
                                        "logically equivalent to a remove operation followed by an add operation " +
                                        "to the same path or to an add operation to an existing member.",
                                    Label = "Replace",
                                    Value = "replace",
                                    Name = "replace",
                                },
                                new SelectOptionValueItemRepresentation
                                {
                                    Type = FormType.Enum,
                                    Description =
                                        "The move operation removes the value at a specified location and adds it " +
                                        "to the target location. The removal location is given by a from member and " +
                                        "the target location is given by the path member.",
                                    Label = "Move",
                                    Value = "move",
                                    Name = "move",
                                },
                                new SelectOptionValueItemRepresentation
                                {
                                    Type = FormType.Enum,
                                    Description =
                                        "Copy is like move. It copies the value at the from location to the path " +
                                        "location, leaving duplicates of the data at each location.",
                                    Label = "Copy",
                                    Value = "copy",
                                    Name = "copy",
                                },
                                new SelectOptionValueItemRepresentation
                                {
                                    Type = FormType.Enum,
                                    Description =
                                        "The HTTP PATCH method is atomic and the patch should only be applied if all " +
                                        "operations can be safely applied. The test operation can offer additional " +
                                        "validation to ensure that patch preconditions or postconditions are met. " +
                                        "If the test fails the whole patch is discarded. test is strictly an " +
                                        "equality check.",
                                    Label = "Test",
                                    Value = "test",
                                    Name = "test",
                                },
                            }
                        },

                        new JsonPointerInputFormItemRepresentation()
                        {
                            Name = "path",
                            Description =
                                "The path member is a JSON Pointer that determines a location within the JSON " +
                                "document to modify.",
                            Required = true
                        },
                        new TextInputFormItemRepresentation
                        {
                            Name = "value",
                            Description = "A primitive (eg number or string) or an object (eg {id:'5'})",
                            Required = false
                        },
                    }
                }
            };
        }
    }
}