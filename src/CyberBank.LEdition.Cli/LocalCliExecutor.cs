using CyberBank.LEdition.Domain;
using CyberBank.LEdition.Infrastructure;

namespace CyberBank.LEdition.Cli;

public sealed class LocalCliExecutor
{
    public async Task<LocalCliExecutionResult> ExecuteAsync(string[] arguments, CancellationToken cancellationToken = default)
    {
        var parsed = LocalCliParser.Parse(arguments);
        if (!parsed.IsAccepted)
        {
            return new LocalCliExecutionResult(false, parsed.Message);
        }

        var projectPath = ReadOption(arguments, "--project")!;

        try
        {
            return parsed.CommandName.ToLowerInvariant() switch
            {
                "project create" => await CreateProjectAsync(arguments, projectPath, cancellationToken),
                "project validate" => await ValidateProjectAsync(projectPath, cancellationToken),
                "evidence import" => await ImportEvidenceAsync(arguments, projectPath, cancellationToken),
                "report export" => await ExportReportAsync(arguments, projectPath, cancellationToken),
                _ => new LocalCliExecutionResult(false, "This command is not supported by the local project and evidence CLI.")
            };
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return new LocalCliExecutionResult(false, exception.Message);
        }
    }

    private static async Task<LocalCliExecutionResult> CreateProjectAsync(
        IReadOnlyList<string> arguments,
        string projectPath,
        CancellationToken cancellationToken)
    {
        if (File.Exists(Path.Combine(projectPath, "l-edition.project.json")))
        {
            return new LocalCliExecutionResult(false, "A local engagement manifest already exists at --project. Refusing to overwrite it.");
        }

        var name = ReadRequiredLocalMetadata(arguments, "--name", "A non-empty --name is required for project create.");
        var authorization = ReadRequiredLocalMetadata(arguments, "--authorization", "A non-empty --authorization is required for project create.");
        var scopeLabel = ReadRequiredLocalMetadata(arguments, "--scope-label", "A non-empty non-target --scope-label is required for project create.");
        if (LooksLikeRemoteOrTarget(scopeLabel))
        {
            return new LocalCliExecutionResult(false, "--scope-label must be a declared local scope label, not a URL, IP address, or target.");
        }

        var engagement = Engagement.Create(name, authorization, [scopeLabel], []);
        var store = new LocalProjectStore(projectPath);
        await store.SaveAsync(engagement);
        return new LocalCliExecutionResult(true, "Local project created with declared authorization and scope. No target operation was performed.");
    }

    private static async Task<LocalCliExecutionResult> ValidateProjectAsync(string projectPath, CancellationToken cancellationToken)
    {
        var snapshot = await new LocalProjectStore(projectPath).LoadProjectAsync();
        if (snapshot.Engagement.ScopeReadiness != ScopeReadiness.Ready)
        {
            return new LocalCliExecutionResult(false, "The local project manifest does not contain a ready declared scope.");
        }

        return new LocalCliExecutionResult(true, "Local project manifest is valid and has declared authorization and scope.");
    }

    private static async Task<LocalCliExecutionResult> ImportEvidenceAsync(
        IReadOnlyList<string> arguments,
        string projectPath,
        CancellationToken cancellationToken)
    {
        var evidencePath = ReadOption(arguments, "--file")!;
        var store = new LocalProjectStore(projectPath);
        var snapshot = await store.LoadProjectAsync();
        var evidence = await new LocalEvidenceImporter().ImportAsync(snapshot.Engagement, evidencePath, projectPath);
        await store.SaveProjectAsync(
            snapshot.Engagement,
            snapshot.Assets,
            snapshot.Evidence.Append(evidence).ToArray(),
            snapshot.Findings);
        return new LocalCliExecutionResult(true, $"Local evidence imported and hashed: {evidence.OriginalFileName} ({evidence.Sha256}). No content was executed.");
    }

    private static async Task<LocalCliExecutionResult> ExportReportAsync(
        IReadOnlyList<string> arguments,
        string projectPath,
        CancellationToken cancellationToken)
    {
        var outputPath = ReadOption(arguments, "--output")!;
        var snapshot = await new LocalProjectStore(projectPath).LoadProjectAsync();
        var reportSnapshot = await new LocalReportExporter().ExportAsync(
            snapshot.Engagement,
            snapshot.Assets,
            snapshot.Evidence,
            snapshot.Findings,
            projectPath,
            outputPath);
        return new LocalCliExecutionResult(true, $"Local report exported: {reportSnapshot.MarkdownPath} | SHA-256: {reportSnapshot.Sha256}");
    }

    private static string ReadRequiredLocalMetadata(IReadOnlyList<string> arguments, string option, string message)
    {
        var value = ReadOption(arguments, option);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, option);
        }

        return value.Trim();
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

    private static bool LooksLikeRemoteOrTarget(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && !uri.IsFile
            || System.Net.IPAddress.TryParse(value, out _);
    }
}
