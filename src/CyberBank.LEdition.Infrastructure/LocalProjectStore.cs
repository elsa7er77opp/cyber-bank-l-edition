using System.Text.Json;
using CyberBank.LEdition.Domain;

namespace CyberBank.LEdition.Infrastructure;

public sealed class LocalProjectStore
{
    private const string ManifestFileName = "l-edition.project.json";
    private const string ActivityFileName = "activity.jsonl";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly string projectRoot;

    public LocalProjectStore(string projectRoot)
    {
        this.projectRoot = projectRoot;
    }

    public async Task SaveAsync(Engagement engagement)
    {
        await SaveProjectAsync(engagement, [], [], []);

        var activity = new
        {
            action = "EngagementCreated",
            occurredUtc = DateTimeOffset.UtcNow
        };

        var activityPath = Path.Combine(projectRoot, ActivityFileName);
        await File.AppendAllTextAsync(
            activityPath,
            JsonSerializer.Serialize(activity) + Environment.NewLine);
    }

    public async Task SaveProjectAsync(
        Engagement engagement,
        IReadOnlyList<AssetRecord> assets,
        IReadOnlyList<EvidenceItem> evidence,
        IReadOnlyList<Finding> findings)
    {
        ArgumentNullException.ThrowIfNull(engagement);
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(findings);

        Directory.CreateDirectory(projectRoot);

        var manifest = new
        {
            engagement = new
            {
                engagement.Name,
                engagement.AuthorizationReference,
                engagement.InScopeEntries,
                engagement.OutOfScopeEntries
            },
            assets = assets.Select(asset => new
            {
                asset.Label,
                asset.Category,
                asset.InScopeReference
            }),
            evidence = evidence.Select(item => new
            {
                item.OriginalFileName,
                item.Sha256,
                item.ByteLength,
                item.StoredPath,
                item.ImportedUtc
            }),
            findings = findings.Select(finding => new
            {
                finding.Title,
                finding.Severity,
                finding.Impact,
                finding.Remediation,
                finding.ReviewStatus,
                finding.EvidenceReferences
            })
        };

        var manifestPath = Path.Combine(projectRoot, ManifestFileName);
        await File.WriteAllTextAsync(
            manifestPath,
            JsonSerializer.Serialize(manifest, JsonOptions));
    }

    public async Task<Engagement> LoadAsync()
    {
        return (await LoadProjectAsync()).Engagement;
    }

    public async Task<LocalProjectSnapshot> LoadProjectAsync()
    {
        var manifestPath = Path.Combine(projectRoot, ManifestFileName);
        await using var stream = File.OpenRead(manifestPath);
        using var manifest = await JsonDocument.ParseAsync(stream);

        var engagement = manifest.RootElement.GetProperty("engagement");
        var inScopeEntries = engagement
            .GetProperty("inScopeEntries")
            .EnumerateArray()
            .Select(entry => entry.GetString() ?? string.Empty)
            .ToArray();
        var outOfScopeEntries = engagement
            .GetProperty("outOfScopeEntries")
            .EnumerateArray()
            .Select(entry => entry.GetString() ?? string.Empty)
            .ToArray();

        var reopenedEngagement = Engagement.Create(
            engagement.GetProperty("name").GetString() ?? string.Empty,
            engagement.GetProperty("authorizationReference").GetString(),
            inScopeEntries,
            outOfScopeEntries);

        var assets = manifest.RootElement.TryGetProperty("assets", out var assetsElement)
            ? assetsElement.EnumerateArray()
                .Select(asset => AssetRecord.Create(
                    reopenedEngagement,
                    asset.GetProperty("label").GetString() ?? string.Empty,
                    asset.GetProperty("category").GetString() ?? string.Empty,
                    asset.GetProperty("inScopeReference").GetString() ?? string.Empty))
                .ToArray()
            : [];

        var evidence = manifest.RootElement.TryGetProperty("evidence", out var evidenceElement)
            ? evidenceElement.EnumerateArray()
                .Select(item => new EvidenceItem(
                    item.GetProperty("originalFileName").GetString() ?? string.Empty,
                    item.GetProperty("sha256").GetString() ?? string.Empty,
                    item.GetProperty("byteLength").GetInt64(),
                    item.GetProperty("storedPath").GetString() ?? string.Empty,
                    item.GetProperty("importedUtc").GetDateTimeOffset()))
                .ToArray()
            : [];

        var findings = manifest.RootElement.TryGetProperty("findings", out var findingsElement)
            ? findingsElement.EnumerateArray()
                .Select(finding => RehydrateFinding(finding))
                .ToArray()
            : [];

        return new LocalProjectSnapshot(reopenedEngagement, assets, evidence, findings);
    }

    private static Finding RehydrateFinding(JsonElement element)
    {
        var finding = Finding.Create(
            element.GetProperty("title").GetString(),
            element.GetProperty("severity").GetString() ?? string.Empty,
            element.GetProperty("impact").GetString() ?? string.Empty,
            element.GetProperty("remediation").GetString() ?? string.Empty);

        if (element.TryGetProperty("evidenceReferences", out var evidenceReferences))
        {
            foreach (var reference in evidenceReferences.EnumerateArray())
            {
                finding.LinkEvidence(reference.GetString() ?? string.Empty);
            }
        }

        return finding;
    }
}
