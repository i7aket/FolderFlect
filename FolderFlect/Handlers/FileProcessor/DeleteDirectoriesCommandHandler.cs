using FolderFlect.Commands.FileProcessor;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using MediatR;

namespace FolderFlect.Handlers.FileProcessor;

public class DeleteDirectoriesCommandHandler : IRequestHandler<DeleteDirectoriesCommand, FileProcessorResult>
{
    private readonly IFileProcessorService _fileProcessorService;

    public DeleteDirectoriesCommandHandler(IFileProcessorService fileProcessorService)
    {
        _fileProcessorService = fileProcessorService;
    }

    public async Task<FileProcessorResult> Handle(DeleteDirectoriesCommand request, CancellationToken cancellationToken)
    {
        return await _fileProcessorService.DeleteDirectoriesAsync(request.AbsolutePathsToDelete);
    }
}
