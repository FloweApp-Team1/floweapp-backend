using IdentityService.Common.Responses;
using IdentityService.Common.Result;

namespace IdentityService.Common.Extensions
{
    public static class ResultExtensions
    {
        public static IResult ToMinimalApiResult<T>(this Result<T> result, string successMessage = "Success")
        {
           
            if (result.IsSuccess)
                return ApiResponse<T>.Success(result.Value, successMessage).ToHttpResult();

            var statusCode = MapErrorToStatusCode(result.Error.Code);
            var apiError = new ApiError(result.Error.Message, result.Error.Code);

            return ApiResponse<T>.Fail(result.Error.Message, statusCode, [apiError]).ToHttpResult();
        }

      
        public static IResult ToMinimalApiResult(this IdentityService.Common.Result.Result result, string successMessage = "Success")
        {
            if (result.IsSuccess)
                return ApiResponse<object>.Success(null, successMessage).ToHttpResult();

            var statusCode = MapErrorToStatusCode(result.Error.Code);
            var apiError = new ApiError(result.Error.Message, result.Error.Code);

            return ApiResponse<object>.Fail(result.Error.Message, statusCode, [apiError]).ToHttpResult();
        }

        private static int MapErrorToStatusCode(string errorCode) => errorCode switch
        {
            _ when errorCode.Contains("TooManyAttempts") => StatusCodes.Status429TooManyRequests,
            _ when errorCode.Contains("InvalidCredentials") => StatusCodes.Status401Unauthorized,
            _ when errorCode.Contains("RefreshToken") => StatusCodes.Status401Unauthorized, 
            _ => StatusCodes.Status400BadRequest
        };
    }
}