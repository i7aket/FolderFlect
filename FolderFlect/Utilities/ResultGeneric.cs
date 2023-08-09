namespace FolderFlect.Utilities
{
    public class Result<T> : Result
    {
        public T Value { get; }

        public Result(T value, bool isSuccess, string message)
            : base(isSuccess, message)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, string.Empty);
        public static Result<T> Success(T value, string message) => new Result<T>(value, true, message);
        public static Result<T> Fail(string message) => new Result<T>(default, false, message);
    }
}