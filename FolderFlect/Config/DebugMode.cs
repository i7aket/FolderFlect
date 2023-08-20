namespace FolderFlect.Config;

public static class DebugMode
{
    public static readonly bool IsEnabled = false;

    public static string[] GetArgs()
    {
        return new string[]
        {
                "-source", @"C:\FolderFlect\ToReplicate",
                "-replica", @"C:\FolderFlect\Reflection",
                "-interval", "10",
                "-log", @"C:\FolderFlect\log.txt"
        };
    }
}

