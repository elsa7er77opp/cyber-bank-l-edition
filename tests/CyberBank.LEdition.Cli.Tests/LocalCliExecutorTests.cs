using CyberBank.LEdition.Cli;
using System.Text.Json;
using Xunit;

namespace CyberBank.LEdition.Cli.Tests;

public sealed class LocalCliExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_project_create_writes_a_valid_local_engagement_manifest()
    {
        var projectPath = Path.Combine(Path.GetTempPath(), "l-edition-cli-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var result = await new LocalCliExecutor().ExecuteAsync(
            [
                "project", "create",
                "--project", projectPath,
                "--name", "Authorized review",
                "--authorization", "AUTH-CLI-001",
                "--scope-label", "documented application review"
            ]);

            Assert.True(result.Succeeded);
            Assert.True(File.Exists(Path.Combine(projectPath, "l-edition.project.json")));
            Assert.True(File.Exists(Path.Combine(projectPath, "activity.jsonl")));
            Assert.Contains("local project created", result.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(projectPath))
            {
                Directory.Delete(projectPath, recursive: true);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_validates_imports_and_exports_only_local_project_records()
    {
        var root = Path.Combine(Path.GetTempPath(), "l-edition-cli-tests", Guid.NewGuid().ToString("N"));
        var projectPath = Path.Combine(root, "project");
        var outputPath = Path.Combine(root, "reports");
        var sourceArtifactPath = Path.Combine(root, "evidence.cmd");
        var executionMarkerPath = Path.Combine(root, "executed.marker");
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(sourceArtifactPath, $"echo unsafe > {executionMarkerPath}");

        try
        {
            var executor = new LocalCliExecutor();
            var createResult = await executor.ExecuteAsync(
            [
                "project", "create",
                "--project", projectPath,
                "--name", "Authorized review",
                "--authorization", "AUTH-CLI-002",
                "--scope-label", "declared product documentation"
            ]);
            var validateResult = await executor.ExecuteAsync(["project", "validate", "--project", projectPath]);
            var importResult = await executor.ExecuteAsync(
            ["evidence", "import", "--project", projectPath, "--file", sourceArtifactPath]);
            var exportResult = await executor.ExecuteAsync(
            ["report", "export", "--project", projectPath, "--output", outputPath]);

            Assert.True(createResult.Succeeded);
            Assert.True(validateResult.Succeeded);
            Assert.True(importResult.Succeeded);
            Assert.True(exportResult.Succeeded);
            Assert.False(File.Exists(executionMarkerPath));
            Assert.Single(Directory.GetFiles(Path.Combine(projectPath, "evidence"), "*.evidence"));
            Assert.Single(Directory.GetFiles(outputPath, "*.json"));
            Assert.Single(Directory.GetFiles(outputPath, "*.md"));

            using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(projectPath, "l-edition.project.json")));
            using var report = JsonDocument.Parse(await File.ReadAllTextAsync(Directory.GetFiles(outputPath, "*.json").Single()));
            Assert.Equal("evidence.cmd", manifest.RootElement.GetProperty("evidence")[0].GetProperty("originalFileName").GetString());
            Assert.Equal("evidence.cmd", report.RootElement.GetProperty("evidence")[0].GetProperty("originalFileName").GetString());
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
