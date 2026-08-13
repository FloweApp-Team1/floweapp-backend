using FluentValidation;

namespace IdentityService.Features.Admin.AdminLogin
{
    
        public sealed class AdminLoginValidator : AbstractValidator<AdminLoginCommand>
        {
            public AdminLoginValidator()
            {
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).NotEmpty();
               
            }
        }
}
