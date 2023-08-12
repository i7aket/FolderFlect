using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace FolderFlect.Utilities
{
    public static class FileSyncHelper
    {
        #region File Operations

        public static void SetFileAsWritable(string absolutePath)
        {
            var file = new FileInfo(absolutePath);
            if (file.IsReadOnly)
                file.IsReadOnly = false;
        }


        public static string CalculateMD5(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var hash = md5.ComputeHash(fileStream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        #endregion

        #region Directory Operations

        public static bool EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                return true; 
            }
            return false; 
        }

        #endregion

        #region Path Operations

        public static string GetRelativePath(string baseDirectory, string targetPath)
        {
            if (!targetPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"The target path {targetPath} does not start with the base directory {baseDirectory}");

            var relativePath = targetPath.Substring(baseDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
            return relativePath;
        }

        #endregion
    }
}
