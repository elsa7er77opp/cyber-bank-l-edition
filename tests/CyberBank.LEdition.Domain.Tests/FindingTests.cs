using System.Reflection;
using Xunit;

namespace CyberBank.LEdition.Domain.Tests;

public sealed class FindingTests
{
    [Fact]
    public void Create_requires_an_assessor_authored_title_and_starts_as_draft()
    {
        var domainAssembly = Assembly.Load("CyberBank.LEdition.Domain");
        var findingType = domainAssembly.GetType("CyberBank.LEdition.Domain.Finding");

        Assert.NotNull(findingType);

        var createMethod = findingType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(createMethod);

        var missingTitleException = Assert.Throws<TargetInvocationException>(() => createMethod.Invoke(
            null,
            [string.Empty, "High", "Business impact supplied by the assessor.", "Local remediation guidance."]));

        Assert.IsType<ArgumentException>(missingTitleException.InnerException);

        var finding = createMethod.Invoke(
            null,
            ["Missing authorization header", "High", "Business impact supplied by the assessor.", "Local remediation guidance."]);

        Assert.NotNull(finding);
        var reviewStatus = findingType.GetProperty("ReviewStatus")?.GetValue(finding)?.ToString();
        Assert.Equal("Draft", reviewStatus);
    }

    [Fact]
    public void Create_preserves_assessor_authored_report_details()
    {
        var domainAssembly = Assembly.Load("CyberBank.LEdition.Domain");
        var findingType = domainAssembly.GetType("CyberBank.LEdition.Domain.Finding");
        Assert.NotNull(findingType);

        var createMethod = findingType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(createMethod);

        var finding = createMethod.Invoke(
            null,
            ["Missing authorization header", "High", "Sensitive data exposure is possible.", "Require authorization for the endpoint."]);

        Assert.NotNull(finding);
        Assert.Equal("Missing authorization header", findingType.GetProperty("Title")?.GetValue(finding));
        Assert.Equal("High", findingType.GetProperty("Severity")?.GetValue(finding)?.ToString());
        Assert.Equal("Sensitive data exposure is possible.", findingType.GetProperty("Impact")?.GetValue(finding));
        Assert.Equal("Require authorization for the endpoint.", findingType.GetProperty("Remediation")?.GetValue(finding));
    }

    [Fact]
    public void LinkEvidence_records_a_local_evidence_reference()
    {
        var domainAssembly = Assembly.Load("CyberBank.LEdition.Domain");
        var findingType = domainAssembly.GetType("CyberBank.LEdition.Domain.Finding");
        Assert.NotNull(findingType);

        var createMethod = findingType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        var linkEvidenceMethod = findingType.GetMethod("LinkEvidence", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(createMethod);
        Assert.NotNull(linkEvidenceMethod);

        var finding = createMethod.Invoke(
            null,
            ["Missing authorization header", "High", "Sensitive data exposure is possible.", "Require authorization for the endpoint."]);
        Assert.NotNull(finding);

        linkEvidenceMethod.Invoke(finding, ["A5E23C"]);

        var references = findingType.GetProperty("EvidenceReferences")?.GetValue(finding);
        Assert.NotNull(references);
        Assert.Contains("A5E23C", Assert.IsAssignableFrom<IEnumerable<string>>(references));
    }
}
