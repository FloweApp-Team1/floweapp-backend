namespace IdentityService.Common.Results
{
    public sealed record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static Error New(string code, string message) => new(code, message);
    }
}
