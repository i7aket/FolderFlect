using FolderFlect.Config;
using FolderFlect.Utilities;
using System.Reflection;

public static class CommandLineParser
{
    public static Result<CommandLineConfig> ParseArgs(string[] args)
    {
        if (args.Length == 0)
        {
            return Result<CommandLineConfig>.Fail(GetUsageDescription());
        }

        var config = new CommandLineConfig();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            string nextArg = (i + 1) < args.Length ? args[i + 1] : null;

            if (string.IsNullOrEmpty(nextArg) || nextArg.StartsWith("-"))
            {
                return Result<CommandLineConfig>.Fail($"Error: Expected a value after '{arg}'.");
            }

            var propToSet = GetPropertyByCmdOption(arg.Substring(1));
            if (propToSet == null)
            {
                return Result<CommandLineConfig>.Fail($"Error: Unknown argument '{arg}'.");
            }

            var attribute = propToSet.GetCustomAttribute<CmdOptionAttribute>();

            if (propToSet.PropertyType == typeof(string))
            {
                propToSet.SetValue(config, nextArg);
                i++;
            }
            else if (propToSet.PropertyType == typeof(int))
            {
                if (!int.TryParse(nextArg, out int value))
                {
                    return Result<CommandLineConfig>.Fail($"Error: {attribute.Description} must be an integer. Received: {nextArg}.");
                }
                propToSet.SetValue(config, value);
                i++;
            }
        }

        if (string.IsNullOrEmpty(config.SourcePath) || string.IsNullOrEmpty(config.ReplicaPath) || config.SyncInterval <= 0 || string.IsNullOrEmpty(config.LogFilePath))
        {
            return Result<CommandLineConfig>.Fail("Error: Some configuration values are missing or invalid.");
        }

        return Result<CommandLineConfig>.Success(config);
    }

    public static string GetUsageDescription()
    {
        string description = "Usage example:\n";
        foreach (var prop in typeof(CommandLineConfig).GetProperties())
        {
            var attribute = prop.GetCustomAttribute<CmdOptionAttribute>();
            if (attribute != null)
            {
                description += $"-{attribute.Key} [{attribute.Description}] ";
            }
        }
        return description.TrimEnd();
    }

    private static PropertyInfo GetPropertyByCmdOption(string optionKey)
    {
        foreach (var prop in typeof(CommandLineConfig).GetProperties())
        {
            var attribute = prop.GetCustomAttribute<CmdOptionAttribute>();
            if (attribute != null && attribute.Key == optionKey)
            {
                return prop;
            }
        }
        return null;
    }

    public static string GetDescriptionFromAttribute(Type type, string propertyName)
    {
        var propertyInfo = type.GetProperty(propertyName);
        var attribute = propertyInfo.GetCustomAttribute<CmdOptionAttribute>();
        return attribute?.Description;
    }
}


[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed class CmdOptionAttribute : Attribute
{
    public string Key { get; }
    public string Description { get; }

    public CmdOptionAttribute(string key, string description)
    {
        Key = key;
        Description = description;
    }
}
