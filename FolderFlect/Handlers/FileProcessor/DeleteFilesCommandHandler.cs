using FolderFlect.Commands.FileProcessor;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using MediatR;

namespace FolderFlect.Handlers.FileProcessor;

public class DeleteFilesCommandHandler : IRequestHandler<DeleteFilesCommand, FileProcessorResult>
{
    private readonly IFileProcessorService _fileProcessorService;

    public DeleteFilesCommandHandler(IFileProcessorService fileProcessorService)
    {
        _fileProcessorService = fileProcessorService;
    }

    public async Task<FileProcessorResult> Handle(DeleteFilesCommand request, CancellationToken cancellationToken)
    {
        return await _fileProcessorService.DeleteFilesAsync(request.AbsolutePathsToDelete);
    }
}
