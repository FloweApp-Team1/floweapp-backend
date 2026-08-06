using FluentValidation;

namespace IdentityService.Features.Auth.LogOut
{
   
        public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
        {
           public LogoutValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
        }
}
