using IdentityService.Common.Results;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Intefaces;
using MediatR;

namespace IdentityService.Features.Admin.AdminLogin.Commands
{
    public sealed record RecordAdminLoginAuditCommand(
         string Email,
         bool IsSuccess,
         string IpAddress,
         string UserAgent) : IRequest<Result>;

    public sealed class RecordAdminLoginAuditHandler(
       
        IIdentityUnitOfWork UOW)
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

            await UOW.AdminLoginAudits.AddAsync(audit, ct);
            await UOW.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
