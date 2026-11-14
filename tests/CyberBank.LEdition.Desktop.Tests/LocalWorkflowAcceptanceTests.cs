using CyberBank.LEdition.Desktop.Security;
using CyberBank.LEdition.Desktop.ViewModels;
using CyberBank.LEdition.Domain;
using CyberBank.LEdition.Infrastructure;
using Xunit;

namespace CyberBank.LEdition.Desktop.Tests;

public sealed class LocalWorkflowAcceptanceTests
{
    [Fact]
    public async Task Local_profile_to_report_workflow_stays_local_and_reopens_full_state()
    {
        var root = Path.Combine(Path.GetTempPath(), "l-edition-acceptance", Guid.NewGuid().ToString("N"));
        var identityRoot = Path.Combine(root, "identity");
        var projectRoot = Path.Combine(root, "project");
        var outputRoot = Path.Combine(root, "reports");
        var artifactPath = Path.Combine(root, "review-note.cmd");
        var executionMarker = Path.Combine(root, "executed.marker");
        Directory.CreateDirectory(root);
        await File.WriteAllTextAsync(artifactPath, $"echo unsafe > {executionMarker}");

        try
        {
            var login = new LocalLoginService(identityRoot);
            var createProfile = await login.CreateProfileAsync("Local Operator", "LimeBlue!2026Secure");
            var unlock = await login.UnlockAsync("LimeBlue!2026Secure");
            Assert.True(createProfile.Succeeded);
            Assert.True(unlock.Succeeded);

            var engagement = Engagement.Create(
                "Acceptance engagement",
                "AUTH-ACCEPTANCE-001",
                ["Approved assessment material"],
                ["Excluded material"]);
            var asset = AssetRecord.Create(
                engagement,
                "Acceptance review record",
                "Assessment material",
                "Approved assessment material");
            var importer = new LocalEvidenceImporter();
            var evidence = await importer.ImportAsync(engagement, artifactPath, projectRoot);
            var finding = Finding.Create(
                "Acceptance finding",
                "Medium",
                "Assessor-authored impact",
                "Assessor-authored remediation");
            finding.LinkEvidence(evidence.Sha256);

            var store = new LocalProjectStore(projectRoot);
            await store.SaveProjectAsync(engagement, [asset], [evidence], [finding]);
            var reopened = await store.LoadProjectAsync();
            var report = await new LocalReportExporter().ExportAsync(
                reopened.Engagement,
                reopened.Assets,
                reopened.Evidence,
                reopened.Findings,
                projectRoot,
                outputRoot);

            var theme = new ThemePreference();
            theme.Toggle();

            Assert.False(File.Exists(executionMarker));
            Assert.Equal("Acceptance review record", Assert.Single(reopened.Assets).Label);
            Assert.Equal("review-note.cmd", Assert.Single(reopened.Evidence).OriginalFileName);
            Assert.Equal("Acceptance finding", Assert.Single(reopened.Findings).Title);
            Assert.True(File.Exists(report.JsonPath));
            Assert.True(File.Exists(report.MarkdownPath));
            Assert.True(theme.IsDark);
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
