using Sprache;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IdentityService.Common.Results
{
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

        public static Result<T> Success<T>(T value) => new(value, true, Error.None);
        public static Result<T> Failure<T>(Error error) => new(default!, false, error);
    }

    public sealed class Result<T> : Result
    {
        private readonly T _value;

        internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
        {
            _value = value;
        }

        public T Value => IsSuccess
            ? _value
            : throw new InvalidOperationException("Cannot access the value of a failed result.");
    }
}
