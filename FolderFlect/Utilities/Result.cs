namespace FolderFlect.Utilities
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        public Result(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message;
        }

        public static Result Success() => new Result(true, string.Empty);
        public static Result Fail(string message) => new Result(false, message);
    }
}