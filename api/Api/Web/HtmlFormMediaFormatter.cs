using System;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Domain.LinkRelations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Toolkit;
using Toolkit.LinkRelations;
using Toolkit.Representation.Forms;
using Toolkit.Representation.LinkedRepresentation;

namespace Api.Web
{
    public class HtmlFormMediaFormatter : TextOutputFormatter
    {
        private const string CreateFormHtml = @"<html>
    <head>
      <title>Create Form</title>
      {1}
    </head>
    <body>
      <form action='{2}' method='POST' enctype='application/x-www-form-urlencoded'>
{0}
        <input type=""submit"" value=""Submit"">
      </form>
    </body>
</html>";

        private const string EditFormHtml = @"<html>
    <head>
      <title>Edit Form</title>
      {1}
    </head>
    <body>
      <form method='PUT' enctype='application/x-www-form-urlencoded'>
{0}
        <input type=""submit"" value=""Update"">
      </form>
    </body>
</html>";

        private const string ResourceHtml = @"<html>
    <head>
      <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8""/>
      <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
      <title>Resource</title>
      {0}
    </head>
    <body>
      <div id=""app"">Intialising ...</div>
      <script src=""http://localhost:8080/dist/api.js""></script>
    </body>
</html>";


        public HtmlFormMediaFormatter()
        {
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedMediaTypes.Add(MediaTypeNames.Text.Html);
        }

        /// <summary>
        ///     This only renders the form representation in a browser. Because a browser
        ///     will allow HTML before JSON having all types available as HTML will
        ///     result in the other data representations trying to be rendered as HTML.
        /// </summary>
        protected override bool CanWriteType(Type type)
        {
            return typeof(CreateFormRepresentation).IsAssignableFrom(type) ||
                   typeof(SearchFormRepresentation).IsAssignableFrom(type) ||
                   typeof(LinkedRepresentation).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            return context.HttpContext.Response.WriteAsync(this.ToHtml(context));
        }

        /// <summary>
        ///     Current implementation supports <see cref = "FormRepresentation" /> which is on just a create form
        ///     There is one upgrade <see cref = "CreateFormRepresentation" /> and others need to be made. There is
        ///     provision for <see cref = "EditFormRepresentation" /> and <see cref = "SearchFormRepresentation" />
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The edit forms are left unchanged as they can't be used natively to edit a resource.
        ///         If the representation had embedded code (code on demand) then this would be possible.
        ///     </para>
        ///     <para>
        ///         Current implementation also supports <see cref = "OptionFormItemRepresentation" /> but shouldn't
        ///         be upgraded use <see cref = "SelectFormItemRepresentation" />
        ///     </para>
        /// </remarks>
        private string ToHtml(OutputFormatterWriteContext context)
        {
            if (context.Object is CreateFormRepresentation ||
                context.Object is SearchFormRepresentation)
            {
                var feed = context.Object as FormRepresentation;

                var submitAction = feed.GetLinkUri(CustomLinkRelation.Submit) ??
                                   feed.GetLinkUri(IanaLinkRelation.Self) ??
                                   feed.GetLinkUri(IanaLinkRelation.Canonical);

                string outerHtml;
                if (feed is CreateFormRepresentation)
                {
                    outerHtml = CreateFormHtml;
                }
                else if (feed is EditFormRepresentation)
                {
                    outerHtml = EditFormHtml;
                }
                else if (feed is SearchFormRepresentation)
                {
                    outerHtml = CreateFormHtml;
                }
                else
                {
                    // Backwards compability
                    outerHtml = CreateFormHtml;
/*
                    Log.ErrorFormat("Unsupported form type");
                    outerHtml = "";
*/
                }

                return string.Format(outerHtml, ToHtml(feed), ToHtmlLinks(feed), submitAction);
            }
            else if (context.Object is LinkedRepresentation)
            {
                var feed = context.Object as LinkedRepresentation;

                return string.Format(ResourceHtml, ToHtmlLinks(feed));
            }
            else
            {
//                Log.Error("Unsupported representation");
            }

            return string.Empty;
        }

        private static string ToHtml(FormRepresentation feed)
        {
            var builder = new StringBuilder();
            var idIndex = 0;
            foreach (var item in feed.Items)
            {
                ToHtml(builder, ref idIndex, "        ", item);
            }

            return builder.ToString();
        }

        private string ToHtmlLinks(LinkedRepresentation r)
        {
            if (r != null && r.Links != null)
            {
                return r.Links.ToString(
                    "",
                    "",
                    "\n      ",
                    link => $@"<link rel=""{link.Rel}"" href=""{link.HRef}"" />");
            }

            return string.Empty;
        }

        private static void ToHtml(StringBuilder builder, ref int idIndex, string prefix, FormItemRepresentation item)
        {
            if (item is TextInputFormItemRepresentation)
            {
                builder
                    .AppendLine(string.Format(
                        @"{0}<label for='{1}'>{2}:</label><br/><input id='{1}' type='text' name='{2}' tooltip='{3}'></input><br/>",
                        prefix,
                        MakeId(ref idIndex, item),
                        item.Name,
                        item.Description));
            }
            else if (item is SelectOptionItemRepresentation)
            {
                // This is provides a text box in HTML for a user to insert a selected url from a collection
                // Users should you the link provided to GET the a collection of urls to select from
                builder.AppendLine(string.Format(
                    @"<label for='{0}'>{1}:</label><br/>
                      <input id='{0}' type='text' name='{1}' tooltip='{2}'/><br/>",
                    item.Id,
                    item.Name,
                    item.Description));
            }
            else if (item is CheckInputFormItemRepresentation)
            {
                builder.AppendLine($@"
                        <label for='{item.Id}'>{item.Name}:</label>
                        <br/>
                        <input id='{item.Id}' 
                                type='checkbox' 
                                value='true' 
                                name='{item.Id}' tooltip='{item.Description}'/>
                        <br/>"
                );
            }
            else if (item is SelectFormItemRepresentation)
            {
                builder
                    .AppendLine(string.Format(
                        @"{0}<label for='{1}'>{2}:</label><br/>
                            <select id='{1}' type='text' name='{2}' tooltip='{3}'{4}>",
                        prefix,
                        MakeId(ref idIndex, item),
                        item.Name,
                        item.Description,
                        item.Multiple ? " multiple" : string.Empty));
                foreach (var i in (item as SelectFormItemRepresentation)?.Items.EmptyIfNull())
                {
                    var option = i as SelectOptionValueItemRepresentation;
                    if (option != null)
                    {
                        builder.Append(prefix).Append("    ")
                            .Append("<option value=\"")
                            .Append(option.Value)
                            .Append("\" ")
                            .Append("title=\"")
                            .Append(option.Description)
                            .Append("\" ")
                            .Append(option.Selected ? "selected" : string.Empty)
                            .Append(">")
                            .Append(option.Label)
                            .AppendLine("</option>");
                    }
                }

                builder
                    .Append(prefix)
                    .AppendLine("</select><br/>");
            }
            else if (item is PasswordInputFormItemRepresentation)
            {
                builder
                    .AppendLine(string.Format(
                        "{0}<label for='{1}'>{2}:</label><br/><input id='{1}' type='password' name='{2}' tooltip='{3}'></input><br/>",
                        prefix,
                        MakeId(ref idIndex, item),
                        item.Name,
                        item.Description));
            }
            else if (item is DateInputFormItemRepresentation)
            {
                builder
                    .AppendLine(string.Format(
                        "{0}<label for='{1}'>{2}:</label><br/><input id='{1}' type='date' name='{2}' tooltip='{3}'></input><br/>",
                        prefix,
                        MakeId(ref idIndex, item),
                        item.Name,
                        item.Description));
            }
            else if (item is DateTimeInputFormItemRepresentation)
            {
                builder
                    .AppendLine($@"{prefix}")
                    .AppendLine($@"<label for='{MakeId(ref idIndex, item)}'>{item.Name}:</label><br/>")
                    .AppendLine($@"<input 
                            id='{MakeId(ref idIndex, item)}' 
                            type='datetime-local' 
                            name='{item.Name}' 
                            tooltip='{item.Description}'></input><br/>");
            }
            else if (item is CollectionInputFormItemRepresentation)
            {
                builder.AppendLine(string.Format(
                    "{0}<label for='{1}'>{2}:</label><br/><input id='{1}' name='{2}' tooltip='{3}'></input><br/>",
                    prefix,
                    MakeId(ref idIndex, item),
                    item.Name,
                    item.Description));
            }
            else if (item is GroupFormItemRepresentation)
            {
                var group = item as GroupFormItemRepresentation;
                builder
                    .Append(prefix).Append("<fieldset name=").Append(group.Name).AppendLine(">")
                    .Append("<legend>").Append(group.Description).AppendLine("</legend>");
                if (group.Items != null)
                {
                    foreach (var gItem in group.Items)
                    {
                        ToHtml(builder, ref idIndex, prefix + "    ", gItem);
                    }
                }

                builder.Append(prefix).AppendLine("</fieldset>");
            }
            else
            {
//                Log.ErrorFormat("Unsupported form item of type {0}", item.GetType().Name);
            }
        }

        private static string MakeId(ref int idIndex, FormItemRepresentation item)
        {
            return !string.IsNullOrEmpty(item.Id) ? item.Id : "L" + ++idIndex;
        }
    }
}