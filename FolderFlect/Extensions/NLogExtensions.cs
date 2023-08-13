using FolderFlect.Models;
using NLog;

namespace FolderFlect.Extensions
{
    public static class NLogExtensions
    {
        public static void LogPathInfo(this ILogger logger, string message, (string Path, string Name) pathInfo)
        {
            logger.Debug($"{message} {pathInfo.Name}: {pathInfo.Path}");
        }

        public static void LogSyncResult(this ILogger logger, List<OperationFileProcessorResult> results)
        {
            foreach (var operationResult in results)
            {
                foreach (var path in operationResult.ProcessorResult.SuccessfullyProcessed)
                {
                    logger.Info($"{operationResult.OperationName} - Successfully processed: {path}");
                }
                foreach (var tuple in operationResult.ProcessorResult.SuccessfullyProcessedTuples)
                {
                    logger.Info($"{operationResult.OperationName} - Successfully processed from {tuple.Source} to {tuple.Destination}");
                }

                foreach (var error in operationResult.ProcessorResult.FailedToProcess)
                {
                    logger.Error($"{operationResult.OperationName} - Error processing {error.Path}. Message: {error.ErrorMessage}");
                }
                foreach (var error in operationResult.ProcessorResult.FailedToProcessTuples)
                {
                    logger.Error($"{operationResult.OperationName} - Error processing from {error.Source} to {error.Destination}. Message: {error.ErrorMessage}");
                }
            }
        }
    }
}
