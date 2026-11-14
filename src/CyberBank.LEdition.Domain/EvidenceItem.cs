namespace CyberBank.LEdition.Domain;

public sealed record EvidenceItem(
    string OriginalFileName,
    string Sha256,
    long ByteLength,
    string StoredPath,
    DateTimeOffset ImportedUtc);
