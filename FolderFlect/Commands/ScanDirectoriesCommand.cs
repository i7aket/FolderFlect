using FolderFlect.Models;
using FolderFlect.Utilities;
using MediatR;

namespace FolderFlect.Commands;

public class ScanDirectoriesCommand : IRequest<Result<MD5FileSet>>
{
}

