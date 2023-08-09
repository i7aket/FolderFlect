using System;
using System.IO;

public static class PathHelper
{
    public static string GetRelativePath(string baseDirectory, string targetPath)
    {
        if (!targetPath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"The target path {targetPath} does not start with the base directory {baseDirectory}");

        var relativePath = targetPath.Substring(baseDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
        return relativePath;
    }
}
