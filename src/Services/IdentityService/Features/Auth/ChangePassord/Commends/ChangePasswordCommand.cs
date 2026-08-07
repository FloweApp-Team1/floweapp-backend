using IdentityService.Common.Contracts;
using IdentityService.Common.Interfaces;
using IdentityService.Common.Models;
using IdentityService.Domain.Entities;
using IdentityService.Features.Auth.ChangePassord.Commends;
using MassTransit.Mediator;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace IdentityService.Features.Auth.ChangePassord.Commends
{
    public record ChangePasswordCommand(
     Guid UserId,
     string CurrentPassword,
     string NewPassword,
     string ConfirmNewPassword): IRequest<Result>;
}
public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public ChangePasswordHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(request.UserId, cancellationToken);

        if (user is null || user.IsDeleted)
            return Result.Failure("User not found.");

        
        var currentCheck = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (currentCheck == PasswordVerificationResult.Failed)
            return Result.Failure("Current password is incorrect.");

        
        var sameAsOld = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.NewPassword);
        if (sameAsOld != PasswordVerificationResult.Failed)
            return Result.Failure("New password must be different from the current password.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        userRepo.Update(user);

        
        var tokenRepo = _unitOfWork.Repository<RefreshToken>();
        var activeTokens = tokenRepo.Query()
            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
            .ToList();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            tokenRepo.Update(token);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // criteria 6: إيميل تأكيد
        await _emailService.SendAsync(
            user.Email,
            "Your password was changed",
            "<p>Your account password was changed successfully. If this wasn't you, please contact support immediately.</p>",
            cancellationToken);

        return Result.Success();
    }
}
