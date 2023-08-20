using FolderFlect.Models;
using MediatR;

namespace FolderFlect.Commands.FileProcessor;

public class CreateDirectoriesCommand : IRequest<FileProcessorResult>
{
    public CreateDirectoriesCommand(List<string> absolutePathsToCreate)
    {
        AbsolutePathsToCreate = absolutePathsToCreate;
    }

    public List<string> AbsolutePathsToCreate { get; set; }
}