namespace CyberBank.LEdition.Cli;

public static class LocalCliParser
{
    private static readonly HashSet<string> SupportedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "project create",
        "project validate",
        "evidence import",
        "report export"
    };

    private static readonly HashSet<string> ProhibitedOptions = new(StringComparer.OrdinalIgnoreCase)
    {
        "--target",
        "--url",
        "--ip",
        "--credential",
        "--payload",
        "--shell",
        "--scanner"
    };

    public static LocalCliParseResult Parse(string[] arguments)
    {
        if (arguments.Length < 2)
        {
            return Reject(string.Empty, "A local L EDITION command is required.");
        }

        var commandName = string.Join(' ', arguments.Take(2));
        var prohibitedOption = arguments.FirstOrDefault(argument => ProhibitedOptions.Contains(argument));
        if (prohibitedOption is not null)
        {
            return Reject(commandName, $"{prohibitedOption} is not supported by this local project and evidence CLI.");
        }

        if (!SupportedCommands.Contains(commandName))
        {
            return Reject(commandName, "This command is not supported by the local project and evidence CLI.");
        }

        var projectPath = ReadOption(arguments, "--project");
        if (string.IsNullOrWhiteSpace(projectPath) || IsRemoteAddress(projectPath))
        {
            return Reject(commandName, "An explicit local --project path is required.");
        }

        if (string.Equals(commandName, "project create", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var requiredOption in new[] { "--name", "--authorization", "--scope-label" })
            {
                if (string.IsNullOrWhiteSpace(ReadOption(arguments, requiredOption)))
                {
                    return Reject(commandName, $"{requiredOption} is required for a declared authorized local project.");
                }
            }
        }

        if (string.Equals(commandName, "evidence import", StringComparison.OrdinalIgnoreCase))
        {
            var evidencePath = ReadOption(arguments, "--file");
            if (string.IsNullOrWhiteSpace(evidencePath) || IsRemoteAddress(evidencePath))
            {
                return Reject(commandName, "An explicit local --file path is required for evidence import.");
            }
        }

        if (string.Equals(commandName, "report export", StringComparison.OrdinalIgnoreCase))
        {
            var outputPath = ReadOption(arguments, "--output");
            if (string.IsNullOrWhiteSpace(outputPath) || IsRemoteAddress(outputPath))
            {
                return Reject(commandName, "An explicit local --output path is required for report export.");
            }
        }

        return new LocalCliParseResult(
            true,
            commandName,
            "Accepted for local project processing. No network, scanner, shell, or target operation will be performed.");
    }

    private static LocalCliParseResult Reject(string commandName, string message)
    {
        return new LocalCliParseResult(false, commandName, message);
    }

    private static string? ReadOption(IReadOnlyList<string> arguments, string option)
    {
        for (var index = 0; index < arguments.Count - 1; index++)
        {
            if (string.Equals(arguments[index], option, StringComparison.OrdinalIgnoreCase))
            {
                return arguments[index + 1];
            }
        }

        return null;
    }

    private static bool IsRemoteAddress(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && !uri.IsFile;
    }
}
