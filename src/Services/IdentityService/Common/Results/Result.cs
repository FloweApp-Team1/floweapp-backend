namespace IdentityService.Common.Results
{
    // The single Result type for the service. It carries a structured Error so
    // ResultExtensions can map a failure onto the right HTTP status code, and it
    // still accepts a plain message for handlers that have nothing richer to say.
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public Error Error { get; }

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("A successful result cannot carry an error.");
            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("A failed result must carry an error.");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);

        public static Result Failure(Error error) => new(false, error);

        public static Result Failure(string message) => new(false, Error.General(message));

        public static Result<T> Success<T>(T value) => new(value, true, Error.None);

        public static Result<T> Failure<T>(Error error) => new(default!, false, error);

        public static Result<T> Failure<T>(string message) => new(default!, false, Error.General(message));
    }

    public sealed class Result<T> : Result
    {
        internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
        {
            Value = value;
        }

        // Only meaningful when IsSuccess is true; default(T) otherwise.
        public T Value { get; }

        public static Result<T> Success(T value) => new(value, true, Error.None);

        public static new Result<T> Failure(Error error) => new(default!, false, error);

        public static new Result<T> Failure(string message) => new(default!, false, Error.General(message));
    }
}
