using Shared.Interfaces;
using Shared.Models;
using Shared.Results;
using MediatR;
using RefreshTokenEntity = IdentityService.Domain.Entities.RefreshToken;

namespace IdentityService.Features.Auth.Sessions.Commends
{

    public record RevokeSessionCommand(Guid UserId, Guid SessionId) : IRequest<Result>;

    public class RevokeSessionHandler : IRequestHandler<RevokeSessionCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        public RevokeSessionHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
        {
            var tokenRepo = _unitOfWork.Repository<RefreshTokenEntity>();

            
            var token = await tokenRepo.FirstOrDefaultAsync(
                t => t.Id == request.SessionId && t.UserId == request.UserId,
                cancellationToken);

            if (token is null)
                return Result.Failure("Session not found.");

           
            token.RevokedAt = DateTime.UtcNow;
            tokenRepo.Update(token);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
