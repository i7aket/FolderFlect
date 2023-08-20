using FolderFlect.Models;
using MediatR;

namespace FolderFlect.Commands.FileProcessor;

public class DeleteDirectoriesCommand : IRequest<FileProcessorResult>
{
    public DeleteDirectoriesCommand(List<string> absolutePathsToDelete)
    {
        AbsolutePathsToDelete = absolutePathsToDelete;
    }

    public List<string> AbsolutePathsToDelete { get; set; }
}
