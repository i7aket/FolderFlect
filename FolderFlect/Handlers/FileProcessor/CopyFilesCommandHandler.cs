using FolderFlect.Commands.FileProcessor;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using MediatR;

namespace FolderFlect.Handlers.FileProcessor;
public class CopyFilesCommandHandler : IRequestHandler<CopyFilesCommand, FileProcessorResult>
{
    private readonly IFileProcessorService _fileProcessorService;

    public CopyFilesCommandHandler(IFileProcessorService fileProcessorService)
    {
        _fileProcessorService = fileProcessorService;
    }

    public async Task<FileProcessorResult> Handle(CopyFilesCommand request, CancellationToken cancellationToken)
    {
        return await _fileProcessorService.CopyFilesAsync(request.AbsolutePathsToCopy);
    }
}
