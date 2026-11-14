using System.Security.Cryptography;
using System.Text.Json;
using CyberBank.LEdition.Domain;

namespace CyberBank.LEdition.Infrastructure;

public sealed class LocalEvidenceImporter
{
    private const string EvidenceFolderName = "evidence";
    private const string ActivityFileName = "activity.jsonl";

    public async Task<EvidenceItem> ImportAsync(
        Engagement engagement,
        string sourceArtifactPath,
        string projectRoot)
    {
        ArgumentNullException.ThrowIfNull(engagement);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceArtifactPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectRoot);

        var evidenceDirectory = Path.Combine(projectRoot, EvidenceFolderName);
        Directory.CreateDirectory(evidenceDirectory);

        var stagedPath = Path.Combine(evidenceDirectory, $"{Guid.NewGuid():N}.evidence");
        await using (var source = File.OpenRead(sourceArtifactPath))
        await using (var destination = File.Create(stagedPath))
        {
            await source.CopyToAsync(destination);
        }

        string sha256;
        await using (var storedStream = File.OpenRead(stagedPath))
        {
            var hash = await SHA256.HashDataAsync(storedStream);
            sha256 = Convert.ToHexString(hash);
        }

        var storedPath = Path.Combine(evidenceDirectory, $"{sha256}.evidence");
        File.Move(stagedPath, storedPath, overwrite: true);

        var activity = new
        {
            action = "EvidenceImported",
            evidenceSha256 = sha256,
            originalFileName = Path.GetFileName(sourceArtifactPath),
            occurredUtc = DateTimeOffset.UtcNow
        };
        await File.AppendAllTextAsync(
            Path.Combine(projectRoot, ActivityFileName),
            JsonSerializer.Serialize(activity) + Environment.NewLine);

        return new EvidenceItem(
            Path.GetFileName(sourceArtifactPath),
            sha256,
            new FileInfo(storedPath).Length,
            storedPath,
            DateTimeOffset.UtcNow);
    }
}
