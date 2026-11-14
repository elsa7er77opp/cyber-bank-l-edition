using System.Reflection;
using System.Text.Json;
using CyberBank.LEdition.Domain;
using Xunit;

namespace CyberBank.LEdition.Infrastructure.Tests;

public sealed class LocalReportExporterTests
{
    [Fact]
    public async Task ExportAsync_creates_local_json_and_markdown_snapshots_with_scope_evidence_findings_and_audit()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), $"l-edition-report-{Guid.NewGuid():N}");
        var projectRoot = Path.Combine(testRoot, "project");
        var outputRoot = Path.Combine(testRoot, "output");
        Directory.CreateDirectory(projectRoot);
        await File.WriteAllTextAsync(
            Path.Combine(projectRoot, "activity.jsonl"),
            "{\"action\":\"EngagementCreated\",\"occurredUtc\":\"2026-08-24T12:00:00+00:00\"}" + Environment.NewLine);

        try
        {
            var infrastructureAssembly = Assembly.Load("CyberBank.LEdition.Infrastructure");
            var exporterType = infrastructureAssembly.GetType("CyberBank.LEdition.Infrastructure.LocalReportExporter");
            Assert.NotNull(exporterType);

            var exporter = Activator.CreateInstance(exporterType);
            var exportMethod = exporterType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .SingleOrDefault(method => method.Name == "ExportAsync" && method.GetParameters().Length == 5);
            Assert.NotNull(exporter);
            Assert.NotNull(exportMethod);

            var engagement = Engagement.Create(
                "Report engagement",
                "AUTH-2026-REPORT",
                ["approved.example"],
                ["excluded.example"]);
            var evidence = new EvidenceItem(
                "assessment-note.md",
                "A5E23C",
                42,
                Path.Combine(projectRoot, "evidence", "A5E23C.evidence"),
                new DateTimeOffset(2026, 8, 24, 12, 0, 0, TimeSpan.Zero));
            var finding = Finding.Create(
                "Missing authorization header",
                "High",
                "Sensitive data exposure is possible.",
                "Require authorization for the endpoint.");
            finding.LinkEvidence("A5E23C");

            var exportTask = Assert.IsAssignableFrom<Task>(exportMethod.Invoke(
                exporter,
                [engagement, new[] { evidence }, new[] { finding }, projectRoot, outputRoot]));
            await exportTask;

            var snapshot = exportTask.GetType().GetProperty("Result")?.GetValue(exportTask);
            Assert.NotNull(snapshot);
            var snapshotType = snapshot.GetType();
            var jsonPath = snapshotType.GetProperty("JsonPath")?.GetValue(snapshot)?.ToString();
            var markdownPath = snapshotType.GetProperty("MarkdownPath")?.GetValue(snapshot)?.ToString();
            var sha256 = snapshotType.GetProperty("Sha256")?.GetValue(snapshot)?.ToString();

            Assert.NotNull(jsonPath);
            Assert.NotNull(markdownPath);
            Assert.Matches("^[A-F0-9]{64}$", sha256);
            Assert.True(File.Exists(jsonPath));
            Assert.True(File.Exists(markdownPath));

            using var json = JsonDocument.Parse(await File.ReadAllTextAsync(jsonPath));
            Assert.Equal("AUTH-2026-REPORT", json.RootElement.GetProperty("engagement").GetProperty("authorizationReference").GetString());
            Assert.Equal("approved.example", json.RootElement.GetProperty("engagement").GetProperty("inScopeEntries")[0].GetString());
            Assert.Equal("A5E23C", json.RootElement.GetProperty("evidence")[0].GetProperty("sha256").GetString());
            Assert.Equal("Missing authorization header", json.RootElement.GetProperty("findings")[0].GetProperty("title").GetString());
            Assert.Equal("Require authorization for the endpoint.", json.RootElement.GetProperty("findings")[0].GetProperty("remediation").GetString());
            Assert.Equal("EngagementCreated", json.RootElement.GetProperty("audit")[0].GetProperty("action").GetString());

            var markdown = await File.ReadAllTextAsync(markdownPath);
            Assert.Contains("Declared Scope", markdown);
            Assert.Contains("A5E23C", markdown);
            Assert.Contains("Missing authorization header", markdown);
            Assert.Contains("Activity Ledger", markdown);
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public async Task ExportAsync_includes_declared_assets_in_local_json_and_markdown()
    {
        var testRoot = Path.Combine(Path.GetTempPath(), $"l-edition-asset-report-{Guid.NewGuid():N}");
        var projectRoot = Path.Combine(testRoot, "project");
        var outputRoot = Path.Combine(testRoot, "output");
        Directory.CreateDirectory(projectRoot);

        try
        {
            var infrastructureAssembly = Assembly.Load("CyberBank.LEdition.Infrastructure");
            var exporterType = infrastructureAssembly.GetType("CyberBank.LEdition.Infrastructure.LocalReportExporter");
            var exportMethod = exporterType?.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .SingleOrDefault(method => method.Name == "ExportAsync" && method.GetParameters().Length == 6);
            Assert.NotNull(exporterType);
            Assert.NotNull(exportMethod);

            var engagement = Engagement.Create("Asset report", "AUTH-ASSET-REPORT", ["declared application review"], []);
            var asset = AssetRecord.Create(
                engagement,
                "Customer portal documentation",
                "Application record",
                "declared application review");
            var exporter = Activator.CreateInstance(exporterType!);
            var exportTask = Assert.IsAssignableFrom<Task>(exportMethod!.Invoke(
                exporter,
                [engagement, new[] { asset }, Array.Empty<EvidenceItem>(), Array.Empty<Finding>(), projectRoot, outputRoot]));
            await exportTask;

            var snapshot = exportTask.GetType().GetProperty("Result")?.GetValue(exportTask);
            var jsonPath = snapshot?.GetType().GetProperty("JsonPath")?.GetValue(snapshot)?.ToString();
            var markdownPath = snapshot?.GetType().GetProperty("MarkdownPath")?.GetValue(snapshot)?.ToString();
            Assert.NotNull(jsonPath);
            Assert.NotNull(markdownPath);

            using var json = JsonDocument.Parse(await File.ReadAllTextAsync(jsonPath));
            Assert.Equal("Customer portal documentation", json.RootElement.GetProperty("assets")[0].GetProperty("label").GetString());

            var markdown = await File.ReadAllTextAsync(markdownPath);
            Assert.Contains("Declared Assets", markdown);
            Assert.Contains("Customer portal documentation", markdown);
        }
        finally
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }
}
