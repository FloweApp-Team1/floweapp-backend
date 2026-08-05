using IdentityService.Common.Result;
using IdentityService.Domain.Intefaces;
using MediatR;

namespace IdentityService.Features.Admin.AdminLogin.Queries
{
    public sealed record CheckAdminRateLimitQuery(string Email, string IpAddress) : IRequest<Result<bool>>;
    public sealed class CheckAdminRateLimitHandler(IAdminLoginAuditRepository auditRepository)
        : IRequestHandler<CheckAdminRateLimitQuery, Result<bool>>
    {
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(15);
        private const int MaxFailedAttempts = 5;

        public async Task<Result<bool>> Handle(CheckAdminRateLimitQuery request, CancellationToken ct)
        {
            var failedByEmail = await auditRepository.CountRecentFailedAttemptsByEmailAsync(request.Email, RateLimitWindow, ct);
            var failedByIp = await auditRepository.CountRecentFailedAttemptsByIpAsync(request.IpAddress, RateLimitWindow, ct);

            var isRateLimited = failedByEmail >= MaxFailedAttempts || failedByIp >= MaxFailedAttempts;
            return Result.Success(isRateLimited);
        }
    }
}
