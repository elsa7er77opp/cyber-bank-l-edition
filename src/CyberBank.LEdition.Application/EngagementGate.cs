using CyberBank.LEdition.Domain;

namespace CyberBank.LEdition.Application;

public sealed class EngagementGate
{
    public Engagement Create(
        string name,
        string? authorizationReference,
        IEnumerable<string> inScopeEntries,
        IEnumerable<string> outOfScopeEntries)
    {
        return Engagement.Create(name, authorizationReference, inScopeEntries, outOfScopeEntries);
    }
}
