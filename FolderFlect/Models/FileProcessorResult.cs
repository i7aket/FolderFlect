namespace FolderFlect.Models
{
    public class FileProcessorResult
    {
        public List<string> SuccessfullyProcessed { get; set; } = new List<string>();
        public List<(string Path, string ErrorMessage)> FailedToProcess { get; set; } = new List<(string Path, string ErrorMessage)>();
        public List<(string Source, string Destination)> SuccessfullyProcessedTuples { get; set; } = new List<(string Source, string Destination)>();
        public List<(string Source, string Destination, string ErrorMessage)> FailedToProcessTuples { get; set; } = new List<(string Source, string Destination, string ErrorMessage)>();

    }
}
