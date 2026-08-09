using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using MediatR;
using RefresheshTokenEntity = IdentityService.Domain.Entities.RefreshToken;

namespace IdentityService.Features.Auth.Sessions.Queries
{
    public record GetActiveSessionsQuery(Guid UserId, string? CurrentRefreshTokenValue) : IRequest<Result<List<SessionDto>>>;

    public class GetActiveSessionsHandler : IRequestHandler<GetActiveSessionsQuery, Result<List<SessionDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public GetActiveSessionsHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public Task<Result<List<SessionDto>>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
        {
            var tokenRepo = _unitOfWork.Repository<RefresheshTokenEntity>();

            var hashedCurrentToken = request.CurrentRefreshTokenValue is null
                ? null
                : _jwtService.HashRefreshTokenValue(request.CurrentRefreshTokenValue);

            var sessions = tokenRepo.Query()
                .Where(t => t.UserId == request.UserId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.LastUsedAt ?? t.CreatedAt)
                .Select(t => new SessionDto(
                    t.Id,
                    t.DeviceName,
                    t.IpAddress,
                    t.Location,
                    t.CreatedAt,
                    t.LastUsedAt,
                    t.Token == hashedCurrentToken))
                .ToList();

            return Task.FromResult(Result<List<SessionDto>>.Success(sessions));
        }
    }
}
