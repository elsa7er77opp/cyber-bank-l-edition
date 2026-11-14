using System.Reflection;
using CyberBank.LEdition.Domain;
using Xunit;

namespace CyberBank.LEdition.Desktop.Tests;

public sealed class WorkstationViewModelTests
{
    [Fact]
    public void CreateAuthorizedEngagement_moves_the_workstation_from_blocked_to_scope_ready()
    {
        var desktopAssembly = Assembly.Load("CyberBank.LEdition.Desktop");
        var viewModelType = desktopAssembly.GetType("CyberBank.LEdition.Desktop.ViewModels.WorkstationViewModel");

        Assert.NotNull(viewModelType);

        var viewModel = Activator.CreateInstance(viewModelType);
        var createMethod = viewModelType.GetMethod("CreateAuthorizedEngagement", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(viewModel);
        Assert.NotNull(createMethod);
        Assert.Equal("AUTHORIZATION REQUIRED", viewModelType.GetProperty("ScopeStatusText")?.GetValue(viewModel));

        createMethod.Invoke(
            viewModel,
            ["Desktop engagement", "AUTH-2026-DESKTOP", "approved.example", "excluded.example"]);

        Assert.Equal("SCOPE READY", viewModelType.GetProperty("ScopeStatusText")?.GetValue(viewModel));
        Assert.Equal(true, viewModelType.GetProperty("HasEngagement")?.GetValue(viewModel));
        Assert.NotNull(viewModelType.GetProperty("CurrentEngagement")?.GetValue(viewModel));
    }

    [Fact]
    public void CreateAuthorizedEngagement_exposes_the_declared_scope_to_the_workstation()
    {
        var desktopAssembly = Assembly.Load("CyberBank.LEdition.Desktop");
        var viewModelType = desktopAssembly.GetType("CyberBank.LEdition.Desktop.ViewModels.WorkstationViewModel");
        Assert.NotNull(viewModelType);

        var viewModel = Activator.CreateInstance(viewModelType);
        var createMethod = viewModelType.GetMethod("CreateAuthorizedEngagement", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(viewModel);
        Assert.NotNull(createMethod);

        createMethod.Invoke(
            viewModel,
            ["Desktop engagement", "AUTH-2026-DESKTOP", "approved.example\napi.approved.example", "excluded.example"]);

        Assert.Equal("Desktop engagement", viewModelType.GetProperty("ActiveEngagementName")?.GetValue(viewModel));
        Assert.Equal("approved.example • api.approved.example", viewModelType.GetProperty("InScopeSummary")?.GetValue(viewModel));
        Assert.Equal("excluded.example", viewModelType.GetProperty("OutOfScopeSummary")?.GetValue(viewModel));
    }

    [Fact]
    public void RegisterEvidence_updates_the_local_evidence_state_visible_to_the_workstation()
    {
        var desktopAssembly = Assembly.Load("CyberBank.LEdition.Desktop");
        var viewModelType = desktopAssembly.GetType("CyberBank.LEdition.Desktop.ViewModels.WorkstationViewModel");
        Assert.NotNull(viewModelType);

        var viewModel = Activator.CreateInstance(viewModelType);
        var registerMethod = viewModelType.GetMethod("RegisterEvidence", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(viewModel);
        Assert.NotNull(registerMethod);

        var evidence = new EvidenceItem(
            "engagement-note.md",
            "A5E23C",
            42,
            "C:\\LocalProject\\evidence\\A5E23C.evidence",
            DateTimeOffset.UtcNow);

        registerMethod.Invoke(viewModel, [evidence]);

        Assert.Equal("1 local evidence item", viewModelType.GetProperty("EvidenceStatusText")?.GetValue(viewModel));
        Assert.Equal("engagement-note.md", viewModelType.GetProperty("LatestEvidenceName")?.GetValue(viewModel));
    }

    [Fact]
    public void RegisterFinding_updates_the_draft_finding_state_visible_to_the_workstation()
    {
        var desktopAssembly = Assembly.Load("CyberBank.LEdition.Desktop");
        var viewModelType = desktopAssembly.GetType("CyberBank.LEdition.Desktop.ViewModels.WorkstationViewModel");
        Assert.NotNull(viewModelType);

        var viewModel = Activator.CreateInstance(viewModelType);
        var registerMethod = viewModelType.GetMethod("RegisterFinding", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(viewModel);
        Assert.NotNull(registerMethod);

        var finding = Finding.Create(
            "Missing authorization header",
            "High",
            "Sensitive data exposure is possible.",
            "Require authorization for the endpoint.");

        registerMethod.Invoke(viewModel, [finding]);

        Assert.Equal("1 draft finding", viewModelType.GetProperty("FindingStatusText")?.GetValue(viewModel));
        Assert.Equal("Missing authorization header", viewModelType.GetProperty("LatestFindingTitle")?.GetValue(viewModel));
    }

    [Fact]
    public void Workstation_exposes_local_evidence_and_finding_metadata_for_a_report_snapshot()
    {
        var viewModel = new CyberBank.LEdition.Desktop.ViewModels.WorkstationViewModel();
        var evidence = new EvidenceItem("note.md", "A5E23C", 42, "C:\\project\\evidence\\A5E23C.evidence", DateTimeOffset.UtcNow);
        var finding = Finding.Create("Missing authorization header", "High", "Impact", "Remediation");

        viewModel.RegisterEvidence(evidence);
        viewModel.RegisterFinding(finding);

        Assert.Equal([evidence], viewModel.EvidenceItems);
        Assert.Equal([finding], viewModel.Findings);
    }

    [Fact]
    public void RegisterAsset_updates_the_declared_asset_state_visible_to_the_workstation()
    {
        var viewModel = new CyberBank.LEdition.Desktop.ViewModels.WorkstationViewModel();
        viewModel.CreateAuthorizedEngagement(
            "Authorized review",
            "AUTH-ASSET-VM-001",
            "documented application review",
            string.Empty);
        var asset = AssetRecord.Create(
            viewModel.CurrentEngagement!,
            "Customer portal documentation",
            "Application record",
            "documented application review");

        viewModel.RegisterAsset(asset);

        Assert.Equal("1 declared asset", viewModel.AssetStatusText);
        Assert.Equal("Customer portal documentation", viewModel.LatestAssetLabel);
    }

    [Fact]
    public void Local_activity_ledger_records_engagement_and_evidence_events_for_the_workstation()
    {
        var viewModel = new CyberBank.LEdition.Desktop.ViewModels.WorkstationViewModel();
        viewModel.CreateAuthorizedEngagement(
            "Authorized review",
            "AUTH-ACTIVITY-001",
            "declared application review",
            string.Empty);
        viewModel.RegisterEvidence(new EvidenceItem(
            "review-note.md",
            "A5E23C",
            42,
            "C:\\project\\evidence\\A5E23C.evidence",
            DateTimeOffset.UtcNow));

        Assert.Contains(viewModel.ActivityEntries, entry => entry.Contains("Engagement created", StringComparison.Ordinal));
        Assert.Contains(viewModel.ActivityEntries, entry => entry.Contains("Evidence imported: review-note.md", StringComparison.Ordinal));
    }

    [Fact]
    public void RestoreProject_rehydrates_the_local_assessment_state_after_restart()
    {
        var engagement = Engagement.Create(
            "Restored assessment",
            "AUTH-RESTORE-001",
            ["approved material"],
            ["excluded material"]);
        var asset = AssetRecord.Create(
            engagement,
            "Release review record",
            "Assessment material",
            "approved material");
        var evidence = new EvidenceItem(
            "notes.md",
            "EVIDENCE-HASH",
            64,
            "evidence/EVIDENCE-HASH.evidence",
            DateTimeOffset.UtcNow);
        var finding = Finding.Create(
            "Review required",
            "Medium",
            "Impact",
            "Remediation");
        finding.LinkEvidence(evidence.Sha256);

        var viewModel = new CyberBank.LEdition.Desktop.ViewModels.WorkstationViewModel();
        viewModel.RestoreProject(engagement, [asset], [evidence], [finding]);

        Assert.Equal("Restored assessment", viewModel.ActiveEngagementName);
        Assert.Equal("1 declared asset", viewModel.AssetStatusText);
        Assert.Equal("notes.md", viewModel.LatestEvidenceName);
        Assert.Equal("Review required", viewModel.LatestFindingTitle);
        Assert.Equal("SCOPE READY", viewModel.ScopeStatusText);
    }
}
