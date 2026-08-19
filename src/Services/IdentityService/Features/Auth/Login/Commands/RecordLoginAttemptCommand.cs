using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Auth.Login.Commands
{
    public sealed record RecordLoginAttemptCommand(
         string Email,
         bool IsSuccess,
         string IpAddress) : IRequest<Result>;

    public sealed class RecordLoginAttemptHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<RecordLoginAttemptCommand, Result>
    {
        public async Task<Result> Handle(RecordLoginAttemptCommand request, CancellationToken ct)
        {
            var attempt = new LoginAttempt
            {
                Email = request.Email,
                AttemptedAt = DateTime.UtcNow,
                IsSuccess = request.IsSuccess,
                IpAddress = request.IpAddress ?? "Unknown"
            };

            await unitOfWork.Repository<LoginAttempt>().AddAsync(attempt, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
    }
}
