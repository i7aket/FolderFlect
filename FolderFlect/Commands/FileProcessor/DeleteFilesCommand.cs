using FolderFlect.Models;
using MediatR;

namespace FolderFlect.Commands.FileProcessor;

public class DeleteFilesCommand : IRequest<FileProcessorResult>
{
    public DeleteFilesCommand(List<string> absolutePathsToDelete)
    {
        AbsolutePathsToDelete = absolutePathsToDelete;
    }

    public List<string> AbsolutePathsToDelete { get; set; }
}
