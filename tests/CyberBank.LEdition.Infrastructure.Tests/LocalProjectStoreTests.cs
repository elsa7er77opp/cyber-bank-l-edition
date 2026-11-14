using System.Reflection;
using System.Text.Json;
using CyberBank.LEdition.Domain;
using Xunit;

namespace CyberBank.LEdition.Infrastructure.Tests;

public sealed class LocalProjectStoreTests
{
    [Fact]
    public async Task SaveAsync_writes_an_engagement_manifest_and_created_activity_locally()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), $"l-edition-{Guid.NewGuid():N}");
        Directory.CreateDirectory(projectRoot);

        try
        {
            var infrastructureAssembly = Assembly.Load("CyberBank.LEdition.Infrastructure");
            var storeType = infrastructureAssembly.GetType("CyberBank.LEdition.Infrastructure.LocalProjectStore");

            Assert.NotNull(storeType);

            var store = Activator.CreateInstance(storeType, projectRoot);
            var saveMethod = storeType.GetMethod("SaveAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(store);
            Assert.NotNull(saveMethod);

            var engagement = Engagement.Create(
                "Local authorized engagement",
                "AUTH-2026-LOCAL",
                ["approved.example"],
                ["excluded.example"]);

            var saveTask = Assert.IsAssignableFrom<Task>(saveMethod.Invoke(store, [engagement]));
            await saveTask;

            var manifestPath = Path.Combine(projectRoot, "l-edition.project.json");
            var activityPath = Path.Combine(projectRoot, "activity.jsonl");

            Assert.True(File.Exists(manifestPath));
            Assert.True(File.Exists(activityPath));

            using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
            Assert.Equal("Local authorized engagement", manifest.RootElement.GetProperty("engagement").GetProperty("name").GetString());
            Assert.Equal("AUTH-2026-LOCAL", manifest.RootElement.GetProperty("engagement").GetProperty("authorizationReference").GetString());
            Assert.Equal("approved.example", manifest.RootElement.GetProperty("engagement").GetProperty("inScopeEntries")[0].GetString());
            Assert.Equal("excluded.example", manifest.RootElement.GetProperty("engagement").GetProperty("outOfScopeEntries")[0].GetString());

            var activityLine = Assert.Single(await File.ReadAllLinesAsync(activityPath));
            using var activity = JsonDocument.Parse(activityLine);
            Assert.Equal("EngagementCreated", activity.RootElement.GetProperty("action").GetString());
        }
        finally
        {
            Directory.Delete(projectRoot, recursive: true);
        }
    }

    [Fact]
    public async Task LoadAsync_reopens_the_saved_declared_scope()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), $"l-edition-{Guid.NewGuid():N}");
        Directory.CreateDirectory(projectRoot);

        try
        {
            var infrastructureAssembly = Assembly.Load("CyberBank.LEdition.Infrastructure");
            var storeType = infrastructureAssembly.GetType("CyberBank.LEdition.Infrastructure.LocalProjectStore");
            Assert.NotNull(storeType);

            var store = Activator.CreateInstance(storeType, projectRoot);
            var saveMethod = storeType.GetMethod("SaveAsync", BindingFlags.Public | BindingFlags.Instance);
            var loadMethod = storeType.GetMethod("LoadAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(store);
            Assert.NotNull(saveMethod);
            Assert.NotNull(loadMethod);

            var original = Engagement.Create(
                "Reopen local engagement",
                "AUTH-2026-REOPEN",
                ["approved.example"],
                ["excluded.example"]);

            var saveTask = Assert.IsAssignableFrom<Task>(saveMethod.Invoke(store, [original]));
            await saveTask;

            var loadTask = Assert.IsAssignableFrom<Task>(loadMethod.Invoke(store, null));
            await loadTask;

            var reopened = Assert.IsType<Engagement>(loadTask.GetType().GetProperty("Result")?.GetValue(loadTask));
            Assert.Equal("Reopen local engagement", reopened.Name);
            Assert.Equal("AUTH-2026-REOPEN", reopened.AuthorizationReference);
            Assert.Equal(["approved.example"], reopened.InScopeEntries);
            Assert.Equal(["excluded.example"], reopened.OutOfScopeEntries);
        }
        finally
        {
            Directory.Delete(projectRoot, recursive: true);
        }
    }

    [Fact]
    public async Task Project_snapshot_round_trips_assets_evidence_metadata_and_findings()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), $"l-edition-{Guid.NewGuid():N}");
        Directory.CreateDirectory(projectRoot);

        try
        {
            var infrastructureAssembly = Assembly.Load("CyberBank.LEdition.Infrastructure");
            var storeType = infrastructureAssembly.GetType("CyberBank.LEdition.Infrastructure.LocalProjectStore");
            Assert.NotNull(storeType);

            var store = Activator.CreateInstance(storeType, projectRoot);
            var saveProjectMethod = storeType.GetMethod("SaveProjectAsync", BindingFlags.Public | BindingFlags.Instance);
            var loadProjectMethod = storeType.GetMethod("LoadProjectAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(store);
            Assert.NotNull(saveProjectMethod);
            Assert.NotNull(loadProjectMethod);

            var engagement = Engagement.Create(
                "Restart-safe local engagement",
                "AUTH-2026-SNAPSHOT",
                ["Approved local material"],
                ["Excluded records"]);
            var asset = AssetRecord.Create(
                engagement,
                "Mobile review notes",
                "Assessment material",
                "Approved local material");
            var evidence = new EvidenceItem(
                "review-notes.txt",
                "2A4A0D7A75C8E8B44CE5D42A6A4D8AA1D48B527B0D5B97D5EF9A1C4C2E45A222",
                128,
                "evidence/2A4A0D7A75C8E8B44CE5D42A6A4D8AA1D48B527B0D5B97D5EF9A1C4C2E45A222.evidence",
                DateTimeOffset.Parse("2026-08-24T10:00:00Z"));
            var finding = Finding.Create(
                "Local record requires review",
                "Medium",
                "Assessor-authored impact",
                "Assessor-authored remediation");
            finding.LinkEvidence(evidence.Sha256);

            var saveTask = Assert.IsAssignableFrom<Task>(saveProjectMethod.Invoke(store, [
                engagement,
                new[] { asset },
                new[] { evidence },
                new[] { finding }
            ]));
            await saveTask;

            var loadTask = Assert.IsAssignableFrom<Task>(loadProjectMethod.Invoke(store, null));
            await loadTask;

            var snapshot = loadTask.GetType().GetProperty("Result")?.GetValue(loadTask);
            Assert.NotNull(snapshot);
            var snapshotType = snapshot.GetType();
            var reopenedEngagement = Assert.IsType<Engagement>(snapshotType.GetProperty("Engagement")?.GetValue(snapshot));
            var reopenedAssets = Assert.IsAssignableFrom<IReadOnlyList<AssetRecord>>(snapshotType.GetProperty("Assets")?.GetValue(snapshot));
            var reopenedEvidence = Assert.IsAssignableFrom<IReadOnlyList<EvidenceItem>>(snapshotType.GetProperty("Evidence")?.GetValue(snapshot));
            var reopenedFindings = Assert.IsAssignableFrom<IReadOnlyList<Finding>>(snapshotType.GetProperty("Findings")?.GetValue(snapshot));

            Assert.Equal("Restart-safe local engagement", reopenedEngagement.Name);
            Assert.Equal("Mobile review notes", Assert.Single(reopenedAssets).Label);
            Assert.Equal(evidence.Sha256, Assert.Single(reopenedEvidence).Sha256);
            Assert.Equal("Local record requires review", Assert.Single(reopenedFindings).Title);
            Assert.Equal([evidence.Sha256], Assert.Single(reopenedFindings).EvidenceReferences);
        }
        finally
        {
            Directory.Delete(projectRoot, recursive: true);
        }
    }
}
