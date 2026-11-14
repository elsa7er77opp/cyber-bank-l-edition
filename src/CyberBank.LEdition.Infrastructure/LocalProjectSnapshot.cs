using CyberBank.LEdition.Domain;

namespace CyberBank.LEdition.Infrastructure;

public sealed record LocalProjectSnapshot(
    Engagement Engagement,
    IReadOnlyList<AssetRecord> Assets,
    IReadOnlyList<EvidenceItem> Evidence,
    IReadOnlyList<Finding> Findings);
