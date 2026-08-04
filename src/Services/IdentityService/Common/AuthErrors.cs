using IdentityService.Common.Result;
namespace IdentityService.Common
{
        public static class AuthErrors
        {
            public static readonly Error InvalidCredentials =
            Error.New("Auth.InvalidCredentials", "Email or password is incorrect.");

            public static readonly Error TooManyAttempts =
            Error.New("Auth.TooManyAttempts", "Too many failed login attempts. Please try again later.");

            public static readonly Error RefreshTokenNotFound =
            Error.New("Auth.RefreshTokenNotFound", "Refresh token was not found.");

            public static readonly Error RefreshTokenAlreadyRevoked =
            Error.New("Auth.RefreshTokenAlreadyRevoked", "This session has already been logged out.");

            public static readonly Error RefreshTokenExpired =
            Error.New("Auth.RefreshTokenExpired", "Refresh token has expired.");
        }
}
