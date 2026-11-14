using System.Reflection;
using CyberBank.LEdition.Domain;
using Xunit;

namespace CyberBank.LEdition.Domain.Tests;

public sealed class EngagementAuthorizationTests
{
    [Fact]
    public void Create_rejects_a_missing_authorization_reference()
    {
        var domainAssembly = Assembly.Load("CyberBank.LEdition.Domain");
        var engagementType = domainAssembly.GetType("CyberBank.LEdition.Domain.Engagement");

        Assert.NotNull(engagementType);

        var createMethod = engagementType.GetMethod(
            "Create",
            BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(createMethod);

        var exception = Assert.Throws<TargetInvocationException>(() => createMethod.Invoke(
            null,
            ["Example authorized engagement", null, new[] { "approved.example" }, new[] { "excluded.example" }]));

        var validationError = Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Contains("authorization", validationError.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_rejects_an_empty_in_scope_list()
    {
        var exception = Assert.Throws<ArgumentException>(() => Engagement.Create(
            "Example authorized engagement",
            "AUTH-2026-001",
            Array.Empty<string>(),
            ["excluded.example"]));

        Assert.Equal("inScopeEntries", exception.ParamName);
    }

    [Fact]
    public void Create_preserves_in_scope_and_out_of_scope_entries_separately()
    {
        var engagement = Engagement.Create(
            "Example authorized engagement",
            "AUTH-2026-001",
            ["approved.example", "api.approved.example"],
            ["billing.example", "third-party.example"]);

        var inScopeProperty = typeof(Engagement).GetProperty("InScopeEntries");
        var outOfScopeProperty = typeof(Engagement).GetProperty("OutOfScopeEntries");

        Assert.NotNull(inScopeProperty);
        Assert.NotNull(outOfScopeProperty);
        Assert.Equal(
            ["approved.example", "api.approved.example"],
            Assert.IsAssignableFrom<IEnumerable<string>>(inScopeProperty.GetValue(engagement)));
        Assert.Equal(
            ["billing.example", "third-party.example"],
            Assert.IsAssignableFrom<IEnumerable<string>>(outOfScopeProperty.GetValue(engagement)));
    }

    [Fact]
    public void Valid_declared_scope_exposes_ready_scope_readiness()
    {
        var engagement = Engagement.Create(
            "Example authorized engagement",
            "AUTH-2026-001",
            ["approved.example"],
            ["excluded.example"]);

        var readinessProperty = typeof(Engagement).GetProperty("ScopeReadiness");

        Assert.NotNull(readinessProperty);
        Assert.Equal("Ready", readinessProperty.GetValue(engagement)?.ToString());
    }

    [Fact]
    public void Create_preserves_the_declared_engagement_identity()
    {
        var engagement = Engagement.Create(
            "Example authorized engagement",
            "AUTH-2026-001",
            ["approved.example"],
            ["excluded.example"]);

        var nameProperty = typeof(Engagement).GetProperty("Name");
        var authorizationProperty = typeof(Engagement).GetProperty("AuthorizationReference");

        Assert.NotNull(nameProperty);
        Assert.NotNull(authorizationProperty);
        Assert.Equal("Example authorized engagement", nameProperty.GetValue(engagement));
        Assert.Equal("AUTH-2026-001", authorizationProperty.GetValue(engagement));
    }
}
