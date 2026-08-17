using Shared.Interfaces;
using Shared.Models;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Users.GetProfile;

public class GetProfileQueryHandler(
    ICurrentUserService _currentUser,
    IGenericRepository<User> _repository)
    : IRequestHandler<GetProfileQuery, Result<GetProfileResponse>>
{
    public async Task<Result<GetProfileResponse>> Handle(
        GetProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
        {
            return Result<GetProfileResponse>
                .Failure(Error.New("Users.Unauthorized", "Unauthorized"));
        }

        var user = await _repository.GetByIdAsync(
            userId.Value,
            cancellationToken);

        if (user is null)
        {
            return Result<GetProfileResponse>
                .Failure(Error.New("Users.NotFound", "User not found"));
        }

        return Result<GetProfileResponse>
            .Success(new GetProfileResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Gender,
                user.ImageUrl
            ));
    }
}
