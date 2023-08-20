using FolderFlect.Models;
using MediatR;

namespace FolderFlect.Commands.FileProcessor;
public class CopyFilesCommand : IRequest<FileProcessorResult>
{
    public CopyFilesCommand(List<(string SourcePath, string DestinationPath)> absolutePathsToCopy)
    {
        AbsolutePathsToCopy = absolutePathsToCopy;
    }

    public List<(string SourcePath, string DestinationPath)> AbsolutePathsToCopy { get; set; }
}
