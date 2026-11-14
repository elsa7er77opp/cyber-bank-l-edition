namespace CyberBank.LEdition.Infrastructure;

public sealed record LocalReportSnapshot(
    string JsonPath,
    string MarkdownPath,
    string Sha256);
