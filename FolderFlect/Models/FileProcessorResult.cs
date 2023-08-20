// The FileProcessorResult class provides a detailed breakdown of the outcome of a file operation.
// It categorizes the results into successful and failed operations, for both individual paths and path pairs (source and destination).
namespace FolderFlect.Models;
public class FileProcessorResult
{
    // List of file paths that were processed successfully.
    public List<string> SuccessfullyProcessed { get; set; } = new List<string>();

    // List of file paths that failed processing along with their respective error messages.
    public List<(string Path, string ErrorMessage)> FailedToProcess { get; set; } = new List<(string Path, string ErrorMessage)>();

    // List of path pairs indicating successful operations from a source to a destination.
    public List<(string Source, string Destination)> SuccessfullyProcessedTuples { get; set; } = new List<(string Source, string Destination)>();

    // List of path pairs indicating failed operations from a source to a destination, along with their respective error messages.
    public List<(string Source, string Destination, string ErrorMessage)> FailedToProcessTuples { get; set; } = new List<(string Source, string Destination, string ErrorMessage)>();
}
