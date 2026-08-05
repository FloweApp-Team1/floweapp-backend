using IdentityService.Common.Responses;
using IdentityService.Common.Result;

namespace IdentityService.Common.Extensions
{
   public static class ResultExtensions
        {
        
            public static IResult ToMinimalApiResult<T>(this Result<T> result, string successMessage = "Success")
            {
                if (result.IsSuccess)
                {
                    return ApiResponse<T>.Success(result.Value, successMessage).ToHttpResult();
                }

                
                int statusCode = result.Error.Code.Contains("TooManyAttempts")
                    ? StatusCodes.Status429TooManyRequests
                    : result.Error.Code.Contains("InvalidCredentials")
                        ? StatusCodes.Status401Unauthorized
                        : StatusCodes.Status400BadRequest;

                var apiError = new ApiError(result.Error.Code, result.Error.Message);

                return ApiResponse<T>.Fail(result.Error.Message, statusCode, [apiError]).ToHttpResult();
            }
        }
}
