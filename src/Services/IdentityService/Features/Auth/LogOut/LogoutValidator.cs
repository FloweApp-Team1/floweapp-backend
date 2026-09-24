using FluentValidation;

namespace IdentityService.Features.Auth.LogOut
{
   
        public sealed class LogoutValidator : AbstractValidator<LogoutCommand>
        {
           public LogoutValidator()
           {
               RuleFor(x => x.UserId).NotEmpty();
               RuleFor(x => x.RefreshToken).NotEmpty();
           }
        }
}
