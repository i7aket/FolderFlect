using FolderFlect.Commands.FileProcessor;
using FolderFlect.Models;
using FolderFlect.Services.IServices;
using MediatR;

namespace FolderFlect.Handlers.FileProcessor;
public class MoveFilesCommandHandler : IRequestHandler<MoveFilesCommand, FileProcessorResult>
{
    private readonly IFileProcessorService _fileProcessorService;

    public MoveFilesCommandHandler(IFileProcessorService fileProcessorService)
    {
        _fileProcessorService = fileProcessorService ?? throw new ArgumentNullException(nameof(fileProcessorService));
    }

    public async Task<FileProcessorResult> Handle(MoveFilesCommand request, CancellationToken cancellationToken)
    {
        return await _fileProcessorService.MoveFilesAsync(request.AbsolutePathsToMove);
    }
}


