using Shared.Results;
using MediatR;

namespace IdentityService.Features.Users.UpdateFcmToken;

public sealed record UpdateFcmTokenCommand(string FcmToken) : IRequest<Result<UpdateFcmTokenResponse>>;
