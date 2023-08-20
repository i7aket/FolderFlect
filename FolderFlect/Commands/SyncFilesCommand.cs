using FolderFlect.Models;
using FolderFlect.Utilities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlect.Commands;
public class SyncFilesCommand : IRequest<Result>
{
    public FilesToSyncSetByMD5 FilesToSyncSet { get; }

    public SyncFilesCommand(FilesToSyncSetByMD5 filesToSyncSet)
    {
        FilesToSyncSet = filesToSyncSet ?? throw new ArgumentNullException(nameof(filesToSyncSet));
    }
}
