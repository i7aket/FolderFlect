using FolderFlect.Models;
using MediatR;

namespace FolderFlect.Commands.FileProcessor;
public class MoveFilesCommand : IRequest<FileProcessorResult>
{
    public List<(string SourcePath, string DestinationPath)> AbsolutePathsToMove { get; }

    public MoveFilesCommand(List<(string SourcePath, string DestinationPath)> absolutePathsToMove)
    {
        AbsolutePathsToMove = absolutePathsToMove ?? throw new ArgumentNullException(nameof(absolutePathsToMove));
    }
}

