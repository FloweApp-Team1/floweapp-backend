using IdentityService.Common.Interfaces;
using IdentityService.Common.Results;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Admin.AdminLogin.Commands
{
    public sealed record RecordAdminLoginAuditCommand(
         string Email,
         bool IsSuccess,
         string IpAddress,
         string UserAgent) : IRequest<Result>;

    public sealed class RecordAdminLoginAuditHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<RecordAdminLoginAuditCommand, Result>
    {
        public async Task<Result> Handle(RecordAdminLoginAuditCommand request, CancellationToken ct)
        {
            var audit = new AdminLoginAudit
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                AttemptedAt = DateTime.UtcNow,
                IsSuccess = request.IsSuccess,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent
            };

            await unitOfWork.Repository<AdminLoginAudit>().AddAsync(audit, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
