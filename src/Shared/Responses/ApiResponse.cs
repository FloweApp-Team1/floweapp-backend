using Shared.Requests;
using Microsoft.AspNetCore.Http;

namespace Shared.Responses
{
    // Unified envelope every endpoint returns. Shape is stable across success and
    // failure so the frontend can rely on the same five/six fields every time.
    public class ApiResponse<T>
    {
        public bool Status { get; init; }
        public int Code { get; init; }
        public string Message { get; init; } = string.Empty;
        public T? Data { get; init; }
        public PaginationMeta? Pagination { get; init; }
        public IReadOnlyList<ApiError>? Errors { get; init; }

        // ---------- Success ----------

        public static ApiResponse<T> Success(
            T? data, string message = "Success", int code = StatusCodes.Status200OK) => new()
            {
                Status = true,
                Code = code,
                Message = message,
                Data = data
            };

        public static ApiResponse<T> Created(T? data, string message = "Created") =>
            Success(data, message, StatusCodes.Status201Created);

        // ---------- Failure ----------

        public static ApiResponse<T> Fail(
            string message,
            int code = StatusCodes.Status400BadRequest,
            IReadOnlyList<ApiError>? errors = null) => new()
            {
                Status = false,
                Code = code,
                Message = message,
                Errors = errors
            };

        // Turns the envelope into a minimal-API result carrying the matching HTTP status code.
        public IResult ToHttpResult() =>
            Microsoft.AspNetCore.Http.Results.Json(this, statusCode: Code);
    }

    // Non-generic entry points for responses that carry no typed payload (data is {} / null)
    // and for building the "data + pagination" pair from a PaginatedResult.
    public static class ApiResponse
    {
        public static ApiResponse<T> Success<T>(
            T? data, string message = "Success", int code = StatusCodes.Status200OK) =>
            ApiResponse<T>.Success(data, message, code);

        public static ApiResponse<object> Fail(
            string message,
            int code = StatusCodes.Status400BadRequest,
            IReadOnlyList<ApiError>? errors = null) =>
            ApiResponse<object>.Fail(message, code, errors);

        // Convenience for a single field-level validation error.
        public static ApiResponse<object> ValidationError(string field, string message) =>
            ApiResponse<object>.Fail(
                "Validation failed",
                StatusCodes.Status400BadRequest,
                [new ApiError(message, field)]);

        // Paged list: items land in Data, paging metadata in Pagination.
        public static ApiResponse<IReadOnlyList<T>> Paginated<T>(
            IReadOnlyList<T> items,
            int totalCount,
            PaginationRequest request,
            string message = "Success") => new()
            {
                Status = true,
                Code = StatusCodes.Status200OK,
                Message = message,
                Data = items,
                Pagination = PaginationMeta.Create(totalCount, request.Page, request.PageSize)
            };
    }
}
