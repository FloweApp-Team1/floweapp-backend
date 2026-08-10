using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Features.Admin.AdminLogin.Queries
{
    public sealed record CheckAdminRateLimitQuery(string Email, string IpAddress) : IRequest<Result<bool>>;

    public sealed class CheckAdminRateLimitHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<CheckAdminRateLimitQuery, Result<bool>>
    {
        private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(15);
        private const int MaxFailedAttempts = 5;

        public async Task<Result<bool>> Handle(CheckAdminRateLimitQuery request, CancellationToken ct)
        {
            var since = DateTime.UtcNow.Subtract(RateLimitWindow);

            var audits = unitOfWork.Repository<AdminLoginAudit>().Query()
                .Where(a => !a.IsSuccess && a.AttemptedAt >= since);

            var failedByEmail = await audits.CountAsync(a => a.Email == request.Email, ct);
            var failedByIp = await audits.CountAsync(a => a.IpAddress == request.IpAddress, ct);

            var isRateLimited = failedByEmail >= MaxFailedAttempts || failedByIp >= MaxFailedAttempts;

            return Result.Success(isRateLimited);
        }
    }
}
