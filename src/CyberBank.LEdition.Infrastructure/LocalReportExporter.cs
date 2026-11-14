using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CyberBank.LEdition.Domain;

namespace CyberBank.LEdition.Infrastructure;

public sealed class LocalReportExporter
{
    private const string ActivityFileName = "activity.jsonl";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public async Task<LocalReportSnapshot> ExportAsync(
        Engagement engagement,
        IReadOnlyList<EvidenceItem> evidence,
        IReadOnlyList<Finding> findings,
        string projectRoot,
        string outputDirectory)
    {
        return await ExportAsync(engagement, [], evidence, findings, projectRoot, outputDirectory);
    }

    public async Task<LocalReportSnapshot> ExportAsync(
        Engagement engagement,
        IReadOnlyList<AssetRecord> assets,
        IReadOnlyList<EvidenceItem> evidence,
        IReadOnlyList<Finding> findings,
        string projectRoot,
        string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(engagement);
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        Directory.CreateDirectory(outputDirectory);
        var audit = await ReadLocalAuditAsync(projectRoot);
        var snapshotId = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmssfff");
        var jsonPath = Path.Combine(outputDirectory, $"l-edition-report-{snapshotId}.json");
        var markdownPath = Path.Combine(outputDirectory, $"l-edition-report-{snapshotId}.md");

        var report = new
        {
            generatedUtc = DateTimeOffset.UtcNow,
            engagement = new
            {
                engagement.Name,
                engagement.AuthorizationReference,
                engagement.InScopeEntries,
                engagement.OutOfScopeEntries
            },
            assets,
            evidence,
            findings = findings.Select(finding => new
            {
                finding.Title,
                finding.Severity,
                finding.Impact,
                finding.Remediation,
                finding.ReviewStatus,
                finding.EvidenceReferences
            }),
            audit
        };

        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(report, JsonOptions));
        await File.WriteAllTextAsync(markdownPath, BuildMarkdown(engagement, assets, evidence, findings, audit));

        string sha256;
        await using (var jsonStream = File.OpenRead(jsonPath))
        {
            sha256 = Convert.ToHexString(await SHA256.HashDataAsync(jsonStream));
        }

        return new LocalReportSnapshot(jsonPath, markdownPath, sha256);
    }

    private static async Task<IReadOnlyList<JsonElement>> ReadLocalAuditAsync(string projectRoot)
    {
        var activityPath = Path.Combine(projectRoot, ActivityFileName);
        if (!File.Exists(activityPath))
        {
            return [];
        }

        var entries = new List<JsonElement>();
        foreach (var line in await File.ReadAllLinesAsync(activityPath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            using var document = JsonDocument.Parse(line);
            entries.Add(document.RootElement.Clone());
        }

        return entries;
    }

    private static string BuildMarkdown(
        Engagement engagement,
        IReadOnlyList<AssetRecord> assets,
        IReadOnlyList<EvidenceItem> evidence,
        IReadOnlyList<Finding> findings,
        IReadOnlyList<JsonElement> audit)
    {
        var document = new StringBuilder();
        document.AppendLine("# CYBER BANK L EDITION — Local Assessment Record");
        document.AppendLine();
        document.AppendLine("> This is a local report snapshot of declared scope and imported evidence. It does not claim independent verification or transmit any data.");
        document.AppendLine();
        document.AppendLine("## Engagement");
        document.AppendLine();
        document.AppendLine($"- **Name:** {engagement.Name}");
        document.AppendLine($"- **Authorization reference:** {engagement.AuthorizationReference}");
        document.AppendLine();
        document.AppendLine("## Declared Scope");
        document.AppendLine();
        document.AppendLine("### In scope");
        foreach (var entry in engagement.InScopeEntries)
        {
            document.AppendLine($"- {entry}");
        }

        document.AppendLine();
        document.AppendLine("### Out of scope");
        foreach (var entry in engagement.OutOfScopeEntries)
        {
            document.AppendLine($"- {entry}");
        }

        document.AppendLine();
        document.AppendLine("## Declared Assets");
        document.AppendLine();
        foreach (var asset in assets)
        {
            document.AppendLine($"- **{asset.Label}** — {asset.Category}; in-scope reference: `{asset.InScopeReference}`");
        }

        document.AppendLine();
        document.AppendLine("## Imported Evidence");
        document.AppendLine();
        foreach (var item in evidence)
        {
            document.AppendLine($"- **{item.OriginalFileName}** — SHA-256 `{item.Sha256}`, {item.ByteLength} bytes");
        }

        document.AppendLine();
        document.AppendLine("## Review Findings");
        document.AppendLine();
        foreach (var finding in findings)
        {
            document.AppendLine($"### {finding.Title}");
            document.AppendLine();
            document.AppendLine($"- **Severity:** {finding.Severity}");
            document.AppendLine($"- **Review state:** {finding.ReviewStatus}");
            document.AppendLine($"- **Impact:** {finding.Impact}");
            document.AppendLine($"- **Remediation:** {finding.Remediation}");
            document.AppendLine($"- **Evidence references:** {string.Join(", ", finding.EvidenceReferences)}");
            document.AppendLine();
        }

        document.AppendLine("## Activity Ledger");
        document.AppendLine();
        foreach (var entry in audit)
        {
            document.AppendLine($"- `{entry.GetProperty("action").GetString()}`");
        }

        return document.ToString();
    }
}
