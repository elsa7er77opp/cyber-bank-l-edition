namespace CyberBank.LEdition.Domain;

public sealed class Engagement
{
    private Engagement(
        string name,
        string authorizationReference,
        IReadOnlyList<string> inScopeEntries,
        IReadOnlyList<string> outOfScopeEntries)
    {
        Name = name;
        AuthorizationReference = authorizationReference;
        InScopeEntries = inScopeEntries;
        OutOfScopeEntries = outOfScopeEntries;
    }

    public string Name { get; }

    public string AuthorizationReference { get; }

    public IReadOnlyList<string> InScopeEntries { get; }

    public IReadOnlyList<string> OutOfScopeEntries { get; }

    public ScopeReadiness ScopeReadiness => ScopeReadiness.Ready;

    public static Engagement Create(
        string name,
        string? authorizationReference,
        IEnumerable<string> inScopeEntries,
        IEnumerable<string> outOfScopeEntries)
    {
        if (string.IsNullOrWhiteSpace(authorizationReference))
        {
            throw new ArgumentException("An authorization reference is required.", nameof(authorizationReference));
        }

        if (!inScopeEntries.Any())
        {
            throw new ArgumentException("At least one in-scope entry is required.", nameof(inScopeEntries));
        }

        return new Engagement(
            name,
            authorizationReference,
            inScopeEntries.ToArray(),
            outOfScopeEntries.ToArray());
    }
}
