using System.Reflection;
using Xunit;

namespace CyberBank.LEdition.Application.Tests;

public sealed class EngagementGateTests
{
    [Fact]
    public void Create_returns_a_ready_declared_engagement_for_valid_operator_input()
    {
        var applicationAssembly = Assembly.Load("CyberBank.LEdition.Application");
        var gateType = applicationAssembly.GetType("CyberBank.LEdition.Application.EngagementGate");

        Assert.NotNull(gateType);

        var gate = Activator.CreateInstance(gateType);
        var createMethod = gateType.GetMethod("Create", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(gate);
        Assert.NotNull(createMethod);

        var engagement = createMethod.Invoke(
            gate,
            ["Desktop local engagement", "AUTH-2026-DESKTOP", new[] { "approved.example" }, new[] { "excluded.example" }]);

        Assert.NotNull(engagement);
        Assert.Equal("Ready", engagement.GetType().GetProperty("ScopeReadiness")?.GetValue(engagement)?.ToString());
    }
}
