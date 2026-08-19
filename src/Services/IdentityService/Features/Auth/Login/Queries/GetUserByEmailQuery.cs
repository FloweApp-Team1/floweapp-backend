using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IdentityService.Domain.Enums;

namespace IdentityService.Features.Auth.Login.Queries
{
    public record UserLoginProjection(
        Guid Id,
        string Email,
        string PhoneNumber,
        string FirstName,
        string LastName,
        DateTime CreatedAt,
        GenderEnum Gender,
        NotificationStatusEnum NotificationStatus,
        string PasswordHash,
        bool IsActive,
        string? ImageUrl,
        // Null for anyone who is not a driver. The access token has to carry the driver's
        // approval status, so the login query is the only place it can come from.
        DeliveryStatusEnum? DriverStatus,
        List<string> RoleNames
    );

    public sealed record GetUserByEmailQuery(string Email) : IRequest<Result<UserLoginProjection?>>;

    public sealed class GetUserByEmailHandler(IUnitOfWork unitOfWork)
       : IRequestHandler<GetUserByEmailQuery, Result<UserLoginProjection?>>
    {
        public async Task<Result<UserLoginProjection?>> Handle(GetUserByEmailQuery request, CancellationToken ct)
        {
            var user = await unitOfWork.Repository<User>().Query()
                .AsNoTracking()
                .Where(u => u.Email == request.Email)
                .Select(u => new UserLoginProjection(
                    u.Id,
                    u.Email,
                    u.PhoneNumber,
                    u.FirstName,
                    u.LastName,
                    u.CreatedAt,
                    u.Gender,
                    u.NotificationStatus,
                    u.PasswordHash,
                    u.IsActive,
                    u.ImageUrl,
                    u is Delivery ? ((Delivery)u).Status : (DeliveryStatusEnum?)null,
                    u.UserRoles != null ? u.UserRoles.Select(ur => ur.Role.Name).ToList() : new List<string>()
                ))
                .FirstOrDefaultAsync(ct);

            return Result.Success(user);
        }
    }
}
