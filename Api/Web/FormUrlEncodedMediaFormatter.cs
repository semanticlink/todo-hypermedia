using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FastMember;
using Microsoft.AspNetCore.Mvc.Formatters;
using NLog;

namespace TodoApi.Web
{
    /// <summary>
    ///     <para>
    ///         The default ASP.NET MVC framework will not deserialise <b>both</b> JSON and form-encoded
    ///         data from a single method. This makes it really hard to implement a hypermedia API.
    ///         This input helper implements the derialisation of the form encoded data.
    /// 
    ///         see https://github.com/aspnet/Mvc/issues/3264
    ///         see https://andrewlock.net/model-binding-json-posts-in-asp-net-core/
    ///     </para>
    ///     <para>
    ///         TODO: This input helper requires that the target data class defined by
    ///         TODO: <see cref = "InputFormatterContext.ModelType" /> has a <see cref = "IFormDataReader" />
    ///         TODO: implementation. This code should be enhanced to just use public setters.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is new behaviour by the ASP.NET MVC framework in that both form-encoded data
    ///         and JSON could be deserialised into a parameter before. Now due to 'security' concerns (FUD)
    ///         this has been disabled.
    ///     </para>
    /// </remarks>
    public class FormUrlEncodedMediaFormatter : IInputFormatter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly string FormUrlEncodedMediaType = "application/x-www-form-urlencoded";

        public bool CanRead(InputFormatterContext context)
        {
            return context.HttpContext.Request.ContentType == FormUrlEncodedMediaType;
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context.HttpContext.Request.Form != null && context.HttpContext.Request.Form.Any())
            {
                var modelType = context.ModelType;
                var obj = Activator.CreateInstance(modelType);
                var accessor = TypeAccessor.Create(modelType);
                if (obj != null)
                {
                    var formDictionary = context
                        .HttpContext
                        .Request
                        .Form
                        .ToDictionary(
                            d => d.Key,
                            d => d.Value.ToString());

                    foreach (var entry in formDictionary)
                    {
                        // Hardcoded assumption of en-US that uses title case as per programming convention
                        var property = new CultureInfo("en-US", false).TextInfo.ToTitleCase(entry.Key);
                        try
                        {
                            accessor[obj, property] = entry.Value;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Cannot write property '{0}' on {1}", entry.Key, modelType.Name);
                            Log.Info("Cannot write property '{0}' on {1}", entry.Key, modelType.Name);
                        }
                    }

                    return await InputFormatterResult.SuccessAsync(obj);
                }
            }
            else
            {
                Log.Error("The form collection must exist for this formatter.");
            }

            return InputFormatterResult.Failure();
        }
    }
}