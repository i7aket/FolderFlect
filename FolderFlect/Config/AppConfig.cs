using FolderFlect.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderFlect.Config
{
    public record AppConfig(
        string SourcePath, 
        string ReplicaPath, 
        int SyncInterval, 
        string LogFilePath);

}

