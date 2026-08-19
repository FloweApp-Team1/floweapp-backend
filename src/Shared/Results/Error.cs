namespace Shared.Results
{
    public sealed record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);

        public static Error New(string code, string message) => new(code, message);

        // Fallback code for failures that only carry a message.
        public static Error General(string message) => new("General.Failure", message);
    }
}
