namespace IdentityService.Common.Responses
{
    // A single field-level error, e.g. { "field": "email", "message": "..." }.
    // Field is optional: leave it null for errors that aren't tied to one input.
    public record ApiError(string Message, string? Field = null);
}
