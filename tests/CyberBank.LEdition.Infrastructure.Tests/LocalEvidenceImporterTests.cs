using System.Reflection;
using System.Text.Json;
using CyberBank.LEdition.Domain;
using Xunit;

namespace CyberBank.LEdition.Infrastructure.Tests;

public sealed class LocalEvidenceImporterTests
{
    [Fact]
    public async Task ImportAsync_copies_selected_local_artifact_with_hash_without_executing_it()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), $"l-edition-import-{Guid.NewGuid():N}");
        var projectRoot = Path.Combine(testRoot, "project");
        var sourceRoot = Path.Combine(testRoot, "source");
        Directory.CreateDirectory(projectRoot);
        Directory.CreateDirectory(sourceRoot);

        var sourceArtifactPath = Path.Combine(sourceRoot, "do-not-run.cmd");
        var executionMarkerPath = Path.Combine(sourceRoot, "execution-marker.txt");
        await File.WriteAllTextAsync(sourceArtifactPath, "@echo executed > execution-marker.txt");

        try
        {
            var infrastructureAssembly = Assembly.Load("CyberBank.LEdition.Infrastructure");
            var importerType = infrastructureAssembly.GetType("CyberBank.LEdition.Infrastructure.LocalEvidenceImporter");

            Assert.NotNull(importerType);

            var importer = Activator.CreateInstance(importerType);
            var importMethod = importerType.GetMethod("ImportAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(importer);
            Assert.NotNull(importMethod);

            var engagement = Engagement.Create(
                "Evidence import engagement",
                "AUTH-2026-EVIDENCE",
                ["approved.example"],
                ["excluded.example"]);

            var importTask = Assert.IsAssignableFrom<Task>(importMethod.Invoke(importer, [engagement, sourceArtifactPath, projectRoot]));
            await importTask;

            var evidence = importTask.GetType().GetProperty("Result")?.GetValue(importTask);
            Assert.NotNull(evidence);
            Assert.False(File.Exists(executionMarkerPath));

            var evidenceType = evidence.GetType();
            var originalFileName = evidenceType.GetProperty("OriginalFileName")?.GetValue(evidence)?.ToString();
            var sha256 = evidenceType.GetProperty("Sha256")?.GetValue(evidence)?.ToString();
            var byteLength = evidenceType.GetProperty("ByteLength")?.GetValue(evidence);
            var storedPath = evidenceType.GetProperty("StoredPath")?.GetValue(evidence)?.ToString();

            Assert.Equal("do-not-run.cmd", originalFileName);
            Assert.Matches("^[A-F0-9]{64}$", sha256);
            Assert.Equal(new FileInfo(sourceArtifactPath).Length, Assert.IsType<long>(byteLength));
            Assert.NotNull(storedPath);
            Assert.True(File.Exists(storedPath));
            Assert.Equal(await File.ReadAllTextAsync(sourceArtifactPath), await File.ReadAllTextAsync(storedPath));

            var activityLines = await File.ReadAllLinesAsync(Path.Combine(projectRoot, "activity.jsonl"));
            var activityLine = Assert.Single(activityLines);
            using var activity = JsonDocument.Parse(activityLine);
            Assert.Equal("EvidenceImported", activity.RootElement.GetProperty("action").GetString());
            Assert.Equal(sha256, activity.RootElement.GetProperty("evidenceSha256").GetString());
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }
}
