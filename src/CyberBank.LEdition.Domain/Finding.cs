namespace CyberBank.LEdition.Domain;

public sealed class Finding
{
    private readonly List<string> evidenceReferences = [];

    private Finding(
        string title,
        string severity,
        string impact,
        string remediation)
    {
        Title = title;
        Severity = severity;
        Impact = impact;
        Remediation = remediation;
        ReviewStatus = FindingReviewStatus.Draft;
    }

    public string Title { get; }

    public string Severity { get; }

    public string Impact { get; }

    public string Remediation { get; }

    public FindingReviewStatus ReviewStatus { get; }

    public IReadOnlyList<string> EvidenceReferences => evidenceReferences;

    public static Finding Create(
        string? title,
        string severity,
        string impact,
        string remediation)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A finding title is required.", nameof(title));
        }

        return new Finding(title, severity, impact, remediation);
    }

    public void LinkEvidence(string evidenceReference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceReference);
        evidenceReferences.Add(evidenceReference);
    }
}
