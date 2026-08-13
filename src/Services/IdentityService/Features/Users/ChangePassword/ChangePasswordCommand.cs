using Shared.Contracts;
using Shared.Interfaces;
using Shared.Results;
using IdentityService.Domain.Entities;
using MediatR;

namespace IdentityService.Features.Users.ChangePassword
{
    //public sealed record ChangePasswordCommand(
    //    Guid UserId,
    //    string CurrentPassword,
    //    string NewPassword,
    //    string ConfirmNewPassword) : IRequest<Result>;

    //public sealed class ChangePasswordHandler(
    //    IUnitOfWork unitOfWork,
    //    IPasswordHasher passwordHasher,
    //    IEmailService emailService)
    //    : IRequestHandler<ChangePasswordCommand, Result>
    //{
    //    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    //    {
    //        var userRepo = unitOfWork.Repository<User>();
    //        var user = await userRepo.GetByIdAsync(request.UserId, cancellationToken);

    //        if (user is null || user.IsDeleted)
    //            return Result.Failure("User not found.");

    //        if (!passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
    //            return Result.Failure("Current password is incorrect.");

    //        if (passwordHasher.Verify(request.NewPassword, user.PasswordHash))
    //            return Result.Failure("New password must be different from the current password.");

    //        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
    //        userRepo.Update(user);

    //        // Changing the password ends every existing session.
    //        var tokenRepo = unitOfWork.Repository<RefreshToken>();
    //        var activeTokens = tokenRepo.Query()
    //            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
    //            .ToList();

    //        foreach (var token in activeTokens)
    //        {
    //            token.RevokedAt = DateTime.UtcNow;
    //            tokenRepo.Update(token);
    //        }

    //        await unitOfWork.SaveChangesAsync(cancellationToken);

    //        await emailService.SendAsync(
    //            user.Email,
    //            "Your password was changed",
    //            "<p>Your account password was changed successfully. If this wasn't you, please contact support immediately.</p>",
    //            cancellationToken);

    //        return Result.Success();
    //    }
    //}
    public sealed record ChangePasswordCommand(
        Guid UserId,
        string CurrentPassword,
        string NewPassword,
        string ConfirmNewPassword) : IRequest<Result>;

    public sealed class ChangePasswordHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        ILogger<ChangePasswordHandler> logger)
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

            // This is the operation that actually defines success/failure for the
            // client. Once this commits, the password IS changed - nothing after
            // this point should be able to turn the response into a failure.
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Notification only. If the email provider is down, that's not the
            // caller's problem and must not surface as a 500 for a request that
            // already succeeded - swallow, log, and move on.
            try
            {
                await emailService.SendAsync(
                    user.Email,
                    "Your password was changed",
                    "<p>Your account password was changed successfully. If this wasn't you, please contact support immediately.</p>",
                    // Deliberately not tied to the inbound request's token - same
                    // rationale as the fire-and-forget send in SmtpEmailService.
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Failed to send password-changed notification email to user {UserId}",
                    user.Id);
            }

            return Result.Success();
        }
    }
}
