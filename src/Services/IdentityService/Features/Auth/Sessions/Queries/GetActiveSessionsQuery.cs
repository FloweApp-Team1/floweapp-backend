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
        public GetActiveSessionsHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public Task<Result<List<SessionDto>>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
        {
            var tokenRepo = _unitOfWork.Repository<RefresheshTokenEntity>();

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
                    t.Token == request.CurrentRefreshTokenValue))
                .ToList();

            return Task.FromResult(Result<List<SessionDto>>.Success(sessions));
        }
    }
}
