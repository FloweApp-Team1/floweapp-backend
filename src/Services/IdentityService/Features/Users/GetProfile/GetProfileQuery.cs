using Shared.Results;
using MediatR;

namespace IdentityService.Features.Users.GetProfile;

public sealed record GetProfileQuery() : IRequest<Result<GetProfileResponse>>;
