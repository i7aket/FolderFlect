// The OperationFileProcessorResult class encapsulates the result of a specific file operation,
namespace FolderFlect.Models
{
    public class OperationFileProcessorResult
    {
        // Name of the operation (e.g., "File Copying", "File Deletion").
        public string OperationName { get; set; }

        // Detailed outcome of the file operation.
        public FileProcessorResult ProcessorResult { get; set; }

        public OperationFileProcessorResult(FileProcessorResult processorResult, string operationName)
        {
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            ProcessorResult = processorResult ?? throw new ArgumentNullException(nameof(processorResult));
        }

        // Method that checks if there were any failures during the operation.
        // Returns true if there were any failures, otherwise false.
        public bool HasFailures()
        {
            return ProcessorResult.FailedToProcess.Count > 0 || ProcessorResult.FailedToProcessTuples.Count > 0;
        }
    }
}