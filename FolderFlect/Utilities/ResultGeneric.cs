namespace FolderFlect.Utilities
{
    public class Result<T>
    {
        public T Value { get; }
        public bool IsSuccess { get; }
        public string Message { get; }

        public Result(T value)
        {
            Value = value;
            IsSuccess = true;
            Message = string.Empty;
        }

        public Result(string message)
        {
            Value = default;
            IsSuccess = false;
            Message = message;
        }

        public static Result<T> Success(T value) => new Result<T>(value);
        public static Result<T> Fail(string message) => new Result<T>(message);
    }
}