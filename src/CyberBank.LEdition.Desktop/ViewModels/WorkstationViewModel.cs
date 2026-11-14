using CommunityToolkit.Mvvm.ComponentModel;
using CyberBank.LEdition.Application;
using CyberBank.LEdition.Domain;
using System.Collections.ObjectModel;

namespace CyberBank.LEdition.Desktop.ViewModels;

public partial class WorkstationViewModel : ObservableObject
{
    private readonly EngagementGate engagementGate = new();
    private readonly List<EvidenceItem> evidenceItems = [];
    private readonly List<Finding> findings = [];
    private readonly List<AssetRecord> assets = [];

    [ObservableProperty]
    private string scopeStatusText = "AUTHORIZATION REQUIRED";

    [ObservableProperty]
    private bool hasEngagement;

    [ObservableProperty]
    private string activeEngagementName = "No authorized engagement";

    [ObservableProperty]
    private string inScopeSummary = "No declared in-scope entries";

    [ObservableProperty]
    private string outOfScopeSummary = "No declared out-of-scope entries";

    [ObservableProperty]
    private string evidenceStatusText = "No evidence selected";

    [ObservableProperty]
    private string latestEvidenceName = "No local artifact imported";

    [ObservableProperty]
    private string findingStatusText = "No finding drafts";

    [ObservableProperty]
    private string latestFindingTitle = "No assessor-authored finding yet";

    [ObservableProperty]
    private string assetStatusText = "No declared assets";

    [ObservableProperty]
    private string latestAssetLabel = "No asset label recorded";

    public Engagement? CurrentEngagement { get; private set; }

    public IReadOnlyList<EvidenceItem> EvidenceItems => evidenceItems;

    public IReadOnlyList<Finding> Findings => findings;

    public IReadOnlyList<AssetRecord> Assets => assets;

    public ObservableCollection<string> ActivityEntries { get; } = [];

    public void CreateAuthorizedEngagement(
        string name,
        string authorizationReference,
        string inScopeText,
        string outOfScopeText)
    {
        var engagement = engagementGate.Create(
            name,
            authorizationReference,
            SplitEntries(inScopeText),
            SplitEntries(outOfScopeText));

        HasEngagement = true;
        CurrentEngagement = engagement;
        ActiveEngagementName = engagement.Name;
        InScopeSummary = string.Join(" • ", engagement.InScopeEntries);
        OutOfScopeSummary = string.Join(" • ", engagement.OutOfScopeEntries);
        ScopeStatusText = engagement.ScopeReadiness == Domain.ScopeReadiness.Ready
            ? "SCOPE READY"
            : "AUTHORIZATION REQUIRED";
        RecordActivity($"Engagement created: {engagement.Name}");
    }

    public void RegisterEvidence(EvidenceItem evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        evidenceItems.Add(evidence);
        LatestEvidenceName = evidence.OriginalFileName;
        EvidenceStatusText = $"{evidenceItems.Count} local evidence item";
        RecordActivity($"Evidence imported: {evidence.OriginalFileName}");
    }

    public void RegisterFinding(Finding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);
        findings.Add(finding);
        LatestFindingTitle = finding.Title;
        FindingStatusText = $"{findings.Count} draft finding";
        RecordActivity($"Finding draft added: {finding.Title}");
    }

    public void RegisterAsset(AssetRecord asset)
    {
        ArgumentNullException.ThrowIfNull(asset);
        assets.Add(asset);
        LatestAssetLabel = asset.Label;
        AssetStatusText = $"{assets.Count} declared asset";
        RecordActivity($"Declared asset added: {asset.Label}");
    }

    public void RestoreProject(
        Engagement engagement,
        IEnumerable<AssetRecord> restoredAssets,
        IEnumerable<EvidenceItem> restoredEvidence,
        IEnumerable<Finding> restoredFindings)
    {
        ArgumentNullException.ThrowIfNull(engagement);
        ArgumentNullException.ThrowIfNull(restoredAssets);
        ArgumentNullException.ThrowIfNull(restoredEvidence);
        ArgumentNullException.ThrowIfNull(restoredFindings);

        CurrentEngagement = engagement;
        HasEngagement = true;
        ActiveEngagementName = engagement.Name;
        InScopeSummary = string.Join(" • ", engagement.InScopeEntries);
        OutOfScopeSummary = string.Join(" • ", engagement.OutOfScopeEntries);
        ScopeStatusText = engagement.ScopeReadiness == Domain.ScopeReadiness.Ready
            ? "SCOPE READY"
            : "AUTHORIZATION REQUIRED";

        assets.Clear();
        assets.AddRange(restoredAssets);
        AssetStatusText = assets.Count == 0 ? "No declared assets" : $"{assets.Count} declared asset";
        LatestAssetLabel = assets.LastOrDefault()?.Label ?? "No asset label recorded";

        evidenceItems.Clear();
        evidenceItems.AddRange(restoredEvidence);
        EvidenceStatusText = evidenceItems.Count == 0 ? "No evidence selected" : $"{evidenceItems.Count} local evidence item";
        LatestEvidenceName = evidenceItems.LastOrDefault()?.OriginalFileName ?? "No local artifact imported";

        findings.Clear();
        findings.AddRange(restoredFindings);
        FindingStatusText = findings.Count == 0 ? "No finding drafts" : $"{findings.Count} draft finding";
        LatestFindingTitle = findings.LastOrDefault()?.Title ?? "No assessor-authored finding yet";

        ActivityEntries.Clear();
        RecordActivity($"Project reopened: {engagement.Name}");
    }

    public void RegisterReportExport(string markdownPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(markdownPath);
        RecordActivity($"Report snapshot exported: {Path.GetFileName(markdownPath)}");
    }

    private void RecordActivity(string entry)
    {
        ActivityEntries.Insert(0, entry);
        if (ActivityEntries.Count > 5)
        {
            ActivityEntries.RemoveAt(ActivityEntries.Count - 1);
        }
    }

    private static IEnumerable<string> SplitEntries(string scopeText)
    {
        return scopeText
            .Split(["\r\n", "\n", ","], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(entry => !string.IsNullOrWhiteSpace(entry));
    }
}
