using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using TodoApi.Utils;

namespace TodoApi.Web
{
    /// <summary>
    ///     Support for mapping exception to http status codes for the application.
    /// </summary>
    /// <seealso cref = "ExceptionFilterAttribute" />
    /// <see cref = "https://github.com/ivaylokenov/AspNetCore.Mvc.HttpActionResults" />
    /// <seealso cref = "StatusCodeAndReasonResult" />
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public override void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            if (ex is UnauthorizedAccessException)
            {
                // 403
                context.Result = new ForbidResult();
            }
            else if (ex is InvalidDataException)
            {
                // Log a trace message so we can find and fix these if they are inappropriatly
                // generating the wrong/unexpected exception type.
                Log.TraceExceptionFormat(
                    ex,
                    "Bad request (400) for {0} '{1}': {2}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path);

                // 400
                context.Result = ex.Message.MakeBadRequestResult();
            }
            else if (ex is ObjectNotFoundException)
            {
                // 404
                context.Result = ex.Message.MakeObjectNotFoundResult();
            }
            else if (ex is ConflictException)
            {
                // 409
                Log.DebugExceptionFormat(
                    ex,
                    "Http conflict (409) for {0} '{1}': {2}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path);
                context.Result = ex.Message.MakeConflictResult();
            }
            else if (ex is InvalidOperationException)
            {
                //
                // TODO: 
                // TODO: Some usages of this exception relate to the server having 
                // TODO: invalid ata, and some being due to the request having invalid 
                // TODO: data. All usages of this request should be investigated and fixed. 
                // TODO:
                // 409
                Log.WarnExceptionFormat(
                    ex,
                    "Request generated unexpected exception for {0} '{1}': {2}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path);
                context.Result = ex.Message.MakeConflictResult();
            }
            else if (ex is SqlException)
            {
                var sqlEx = ex as SqlException;

                // Execution Timeout Expired.  The timeout period elapsed prior to completion of the operation or the server is not responding. 
                // ---> System.ComponentModel.Win32Exception (0x80004005): The wait operation timed out
                //
                // 408
                //
                // WARNING: hard cast the int as an unsigned int for comparison, otherwise the 
                // error "CS0652: Comparison to integral constant is useless; the constant is 
                // outside the range of type 'int'"
                if ((uint) ex.HResult == 0x80131904)
                {
                    Log.InfoExceptionFormat(
                        ex,
                        "SQL timeout exception: Server '{0}', error [c={1},n={2},h={3}], procedure {4}:{5}: {6}",
                        sqlEx.Server,
                        sqlEx.Class,
                        sqlEx.Number,
                        sqlEx.HResult,
                        sqlEx.Procedure,
                        sqlEx.LineNumber);
                    context.Result = ex.Message.MakeRequestTimeout();
                }
                else
                {
                    Log.ErrorExceptionFormat(
                        ex,
                        "SQL exception not handled: Server '{0}', error [c={1},n={2},h={3}], procedure {4}:{5}: {6}",
                        sqlEx.Server,
                        sqlEx.Class,
                        sqlEx.Number,
                        sqlEx.HResult,
                        sqlEx.Procedure,
                        sqlEx.LineNumber);
                }
            }
            else
            {
                Log.ErrorExceptionFormat(ex, "Exception not handled by MVC filter: {0}");
            }
        }
    }
}