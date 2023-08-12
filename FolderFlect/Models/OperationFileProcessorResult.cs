using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlect.Models
{
    public class OperationFileProcessorResult
    {
        public string OperationName { get; set; }
        public FileProcessorResult ProcessorResult { get; set; }

        public OperationFileProcessorResult(FileProcessorResult processorResult, string operationName)
        {
            OperationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
            ProcessorResult = processorResult ?? throw new ArgumentNullException(nameof(processorResult));
        }

        public bool HasFailures()
        {
            return ProcessorResult.FailedToProcess.Count > 0 || ProcessorResult.FailedToProcessTuples.Count > 0;
        }
    }
}
