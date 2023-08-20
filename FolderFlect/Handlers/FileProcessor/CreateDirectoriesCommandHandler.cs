using FolderFlect.Commands.FileProcessor;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using MediatR;

namespace FolderFlect.Handlers.FileProcessor;

public class CreateDirectoriesCommandHandler : IRequestHandler<CreateDirectoriesCommand, FileProcessorResult>
{
    private readonly IFileProcessorService _fileProcessorService;

    public CreateDirectoriesCommandHandler(IFileProcessorService fileProcessorService)
    {
        _fileProcessorService = fileProcessorService;
    }

    public async Task<FileProcessorResult> Handle(CreateDirectoriesCommand request, CancellationToken cancellationToken)
    {
        return await _fileProcessorService.CreateDirectoriesAsync(request.AbsolutePathsToCreate);
    }
}