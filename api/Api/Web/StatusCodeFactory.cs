using Microsoft.AspNetCore.Http;

namespace Api.Web
{
    public static class StatusCodeFactory
    {
        public static StatusCodeAndReasonResult MakeBadRequestResult(this string message)
        {
            return new StatusCodeAndReasonResult(
                StatusCodes.Status400BadRequest,
                message);
        }

        public static StatusCodeAndReasonResult MakeObjectNotFoundResult(this string message)
        {
            return new StatusCodeAndReasonResult(
                StatusCodes.Status404NotFound,
                message);
        }

        public static StatusCodeAndReasonResult MakeConflictResult(this string message)
        {
            return new StatusCodeAndReasonResult(
                StatusCodes.Status409Conflict,
                message);
        }

        public static StatusCodeAndReasonResult MakeRequestTimeout(this string message)
        {
            return new StatusCodeAndReasonResult(
                StatusCodes.Status408RequestTimeout,
                message);
        }
    }
}