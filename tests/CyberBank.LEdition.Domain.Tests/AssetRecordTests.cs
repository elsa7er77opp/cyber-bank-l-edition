using CyberBank.LEdition.Domain;
using Xunit;

namespace CyberBank.LEdition.Domain.Tests;

public sealed class AssetRecordTests
{
    [Fact]
    public void Create_preserves_a_declared_asset_label_linked_to_an_in_scope_boundary()
    {
        var engagement = Engagement.Create(
            "Authorized review",
            "AUTH-ASSET-001",
            ["documented application review"],
            []);

        var asset = AssetRecord.Create(
            engagement,
            "Customer portal documentation",
            "Application record",
            "documented application review");

        Assert.Equal("Customer portal documentation", asset.Label);
        Assert.Equal("Application record", asset.Category);
        Assert.Equal("documented application review", asset.InScopeReference);
    }

    [Fact]
    public void Create_rejects_an_asset_reference_not_declared_in_scope()
    {
        var engagement = Engagement.Create(
            "Authorized review",
            "AUTH-ASSET-002",
            ["documented application review"],
            []);

        var exception = Assert.Throws<ArgumentException>(() => AssetRecord.Create(
            engagement,
            "Unrelated record",
            "Application record",
            "undeclared boundary"));

        Assert.Equal("inScopeReference", exception.ParamName);
    }

    [Theory]
    [InlineData("https://example.test")]
    [InlineData("203.0.113.10")]
    public void Create_rejects_url_or_ip_style_asset_labels(string label)
    {
        var engagement = Engagement.Create(
            "Authorized review",
            "AUTH-ASSET-003",
            ["documented application review"],
            []);

        var exception = Assert.Throws<ArgumentException>(() => AssetRecord.Create(
            engagement,
            label,
            "Application record",
            "documented application review"));

        Assert.Equal("label", exception.ParamName);
    }
}
