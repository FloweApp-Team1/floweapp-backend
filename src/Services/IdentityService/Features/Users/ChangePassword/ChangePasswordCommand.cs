using IdentityService.Common.Contracts;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Results;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Users.ChangePassword
{
    public sealed record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword,
        string ConfirmNewPassword) : IRequest<Result>;

    public sealed class ChangePasswordHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEmailService emailService)
        : IRequestHandler<ChangePasswordCommand, Result>
    {
        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userRepo = unitOfWork.Repository<User>();
            var user = await userRepo.GetByIdAsync(request.UserId, cancellationToken);

            if (user is null || user.IsDeleted)
                return Result.Failure("User not found.");

            if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                return Result.Failure("Current password is incorrect.");

            if (passwordHasher.Verify(request.NewPassword, user.PasswordHash))
                return Result.Failure("New password must be different from the current password.");

            user.PasswordHash = passwordHasher.Hash(request.NewPassword);
            userRepo.Update(user);

            // Changing the password ends every existing session.
            var tokenRepo = unitOfWork.Repository<RefreshToken>();
            var activeTokens = tokenRepo.Query()
                .Where(t => t.UserId == user.Id && t.RevokedAt == null)
                .ToList();

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                tokenRepo.Update(token);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            await emailService.SendAsync(
                user.Email,
                "Your password was changed",
                "<p>Your account password was changed successfully. If this wasn't you, please contact support immediately.</p>",
                cancellationToken);

            return Result.Success();
        }
    }
}
