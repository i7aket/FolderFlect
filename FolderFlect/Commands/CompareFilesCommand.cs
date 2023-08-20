using FolderFlect.Models;
using FolderFlect.Utilities;
using MediatR;

namespace FolderFlect.Commands;
public class CompareFilesCommand : IRequest<Result<FilesToSyncSetByMD5>>
{
    public MD5FileSet FileSet { get; }

    public CompareFilesCommand(MD5FileSet fileSet)
    {
        FileSet = fileSet ?? throw new ArgumentNullException(nameof(fileSet));
    }
}

