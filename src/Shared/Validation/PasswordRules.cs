using FluentValidation;

namespace Shared.Validation
{
    public static class PasswordRules
    {
        // Single source of truth for the password policy. Reused by password reset
        // and (once built) registration so the two never drift apart.
        public static IRuleBuilderOptions<T, string> Password<T>(
            this IRuleBuilder<T, string> rule) =>
            rule.NotEmpty()
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .MaximumLength(128).WithMessage("Password must be at most 128 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
    }
}
