using Microsoft.AspNetCore.Http;

namespace SemanticLink.AspNetCore
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

        public static StatusCodeAndReasonResult MakeAccepted(this string message)
        {
            return new StatusCodeAndReasonResult(
                StatusCodes.Status202Accepted,
                message);
        }

        public static StatusCodeAndReasonResult InternalServerError(this string message)
        {
            return new StatusCodeAndReasonResult(
                StatusCodes.Status500InternalServerError,
                message);
        }
    }
}