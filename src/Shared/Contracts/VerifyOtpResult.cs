namespace IdentityService.Common.Contracts;

public sealed class VerifyOtpResult
{
    public bool Success { get; init; }
    
    public string? ResetToken { get; init; }
    public int ExpiresInMinutes { get; init; }
    public bool IsExpired { get; init; }

    public bool MaxAttemptsReached { get; init; }

    public string? ErrorMessage { get; init; }

    public static VerifyOtpResult SuccessResult(string token, int expiresInMinutes) =>
        new()
        {
            Success = true,
            ResetToken = token,
            ExpiresInMinutes = expiresInMinutes
        };

    public static VerifyOtpResult Invalid() =>
        new()
        {
            ErrorMessage = "Invalid verification code."
        };

    public static VerifyOtpResult Expired() =>
        new()
        {
            IsExpired = true,
            ErrorMessage = "Code expired. Please request a new one."
        };

    public static VerifyOtpResult AttemptsExceeded() =>
        new()
        {
            MaxAttemptsReached = true,
            ErrorMessage = "Maximum verification attempts reached."
        };
}