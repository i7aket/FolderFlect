using System.Security.Cryptography;

namespace FolderFlect.Utilities;

public static class FileSyncHelper
{
    #region File Operations

    public static async Task SetFileAsWritableAsync(string absolutePath)
    {
        await Task.Run(() =>
        {
            var file = new FileInfo(absolutePath);
            if (file.IsReadOnly)
                file.IsReadOnly = false;
        });
    }

    public static async Task<string> CalculateMD5Async(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var hash = await md5.ComputeHashAsync(fileStream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    #endregion

    #region Directory Operations

    public static async Task EnsureDirectoryExistsAsync(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            await Task.Run(() => Directory.CreateDirectory(directoryPath));
        }
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

    public static List<string> GetAbsolutePaths(IEnumerable<string> relativePaths, string basePath)
    {
        var absolutePaths = new List<string>();
        foreach (var relativePath in relativePaths)
        {
            absolutePaths.Add(Path.Combine(basePath, relativePath));
        }
        return absolutePaths;
    }

    #endregion
}

