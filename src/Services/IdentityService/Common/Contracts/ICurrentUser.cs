namespace IdentityService.Common.Contracts
{
    // Ambient access to the authenticated user for the current request,
    // read from the ClaimsPrincipal on the HTTP context.
    public interface ICurrentUser
    {
        Guid? UserId { get; }

        string? Email { get; }

        bool IsAuthenticated { get; }
    }
}
