using System.Net;

namespace CyberBank.LEdition.Domain;

public sealed class AssetRecord
{
    private AssetRecord(string label, string category, string inScopeReference)
    {
        Label = label;
        Category = category;
        InScopeReference = inScopeReference;
    }

    public string Label { get; }

    public string Category { get; }

    public string InScopeReference { get; }

    public static AssetRecord Create(
        Engagement engagement,
        string label,
        string category,
        string inScopeReference)
    {
        ArgumentNullException.ThrowIfNull(engagement);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentException.ThrowIfNullOrWhiteSpace(inScopeReference);

        if (LooksLikeUrlOrIp(label))
        {
            throw new ArgumentException("Asset labels must be local assessment labels, not URLs or IP addresses.", nameof(label));
        }

        if (!engagement.InScopeEntries.Contains(inScopeReference, StringComparer.Ordinal))
        {
            throw new ArgumentException("Asset records must link to an existing declared in-scope boundary.", nameof(inScopeReference));
        }

        return new AssetRecord(label.Trim(), category.Trim(), inScopeReference.Trim());
    }

    private static bool LooksLikeUrlOrIp(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && !uri.IsFile
            || IPAddress.TryParse(value, out _);
    }
}
