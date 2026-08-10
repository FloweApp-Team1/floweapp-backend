using Shared.Responses;
using Shared.Results;

namespace Shared.Extensions
{
    public static class ResultExtensions
    {
        public static IResult ToMinimalApiResult<T>(this Result<T> result, string successMessage = "Success")
        {
            if (result.IsSuccess)
                return ApiResponse<T>.Success(result.Value, successMessage).ToHttpResult();

            return ApiResponse<T>.Fail(
                result.Error.Message,
                MapErrorToStatusCode(result.Error.Code),
                [new ApiError(result.Error.Message, result.Error.Code)]).ToHttpResult();
        }

        public static IResult ToMinimalApiResult(this Result result, string successMessage = "Success")
        {
            if (result.IsSuccess)
                return ApiResponse<object>.Success(null, successMessage).ToHttpResult();

            return ApiResponse<object>.Fail(
                result.Error.Message,
                MapErrorToStatusCode(result.Error.Code),
                [new ApiError(result.Error.Message, result.Error.Code)]).ToHttpResult();
        }

        private static int MapErrorToStatusCode(string errorCode) => errorCode switch
        {
            _ when errorCode.Contains("TooManyAttempts") => StatusCodes.Status429TooManyRequests,
            _ when errorCode.Contains("InvalidCredentials") => StatusCodes.Status401Unauthorized,
            _ when errorCode.Contains("Unauthorized") => StatusCodes.Status401Unauthorized,
            _ when errorCode.Contains("RefreshToken") => StatusCodes.Status401Unauthorized,
            _ when errorCode.Contains("Forbidden") => StatusCodes.Status403Forbidden,
            _ when errorCode.Contains("NotFound") => StatusCodes.Status404NotFound,
            _ when errorCode.Contains("Conflict") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
    }
}
