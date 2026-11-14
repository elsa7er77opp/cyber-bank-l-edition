using System.Reflection;
using Xunit;

namespace CyberBank.LEdition.Cli.Tests;

public sealed class LocalCliParserTests
{
    [Fact]
    public void Parse_accepts_project_create_with_an_explicit_local_project_path()
    {
        var cliAssembly = Assembly.Load("CyberBank.LEdition.Cli");
        var parserType = cliAssembly.GetType("CyberBank.LEdition.Cli.LocalCliParser");

        Assert.NotNull(parserType);

        var parseMethod = parserType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(parseMethod);

        var result = parseMethod.Invoke(null,
        [
            new[]
            {
                "project", "create",
                "--project", "C:\\local\\l-edition",
                "--name", "Authorized review",
                "--authorization", "AUTH-CLI-PARSE-001",
                "--scope-label", "declared application review"
            }
        ]);
        Assert.NotNull(result);

        var resultType = result.GetType();
        Assert.Equal(true, resultType.GetProperty("IsAccepted")?.GetValue(result));
        Assert.Equal("project create", resultType.GetProperty("CommandName")?.GetValue(result));
    }

    [Theory]
    [InlineData("--name")]
    [InlineData("--authorization")]
    [InlineData("--scope-label")]
    public void Parse_rejects_project_create_without_required_local_authorization_metadata(string omittedOption)
    {
        var arguments = new List<string>
        {
            "project", "create",
            "--project", "C:\\local\\l-edition",
            "--name", "Authorized review",
            "--authorization", "AUTH-CLI-PARSE-002",
            "--scope-label", "declared application review"
        };
        var optionIndex = arguments.IndexOf(omittedOption);
        arguments.RemoveAt(optionIndex + 1);
        arguments.RemoveAt(optionIndex);

        var result = LocalCliParser.Parse(arguments.ToArray());

        Assert.False(result.IsAccepted);
        Assert.Contains(omittedOption, result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("--target", "approved.example")]
    [InlineData("--url", "https://approved.example")]
    [InlineData("--ip", "203.0.113.10")]
    [InlineData("--credential", "operator:secret")]
    [InlineData("--payload", "anything")]
    [InlineData("--shell", "whoami")]
    [InlineData("--scanner", "config.json")]
    public void Parse_rejects_operational_or_remote_arguments(string prohibitedOption, string prohibitedValue)
    {
        var cliAssembly = Assembly.Load("CyberBank.LEdition.Cli");
        var parserType = cliAssembly.GetType("CyberBank.LEdition.Cli.LocalCliParser");

        Assert.NotNull(parserType);

        var parseMethod = parserType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(parseMethod);

        var result = parseMethod.Invoke(
            null,
            [new[] { "project", "create", "--project", "C:\\Assessments\\Local", prohibitedOption, prohibitedValue }]);
        Assert.NotNull(result);

        var resultType = result.GetType();
        Assert.Equal(false, resultType.GetProperty("IsAccepted")?.GetValue(result));
        Assert.Contains("not supported", resultType.GetProperty("Message")?.GetValue(result)?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("evidence", "import", "--file")]
    [InlineData("report", "export", "--output")]
    public void Parse_requires_the_local_path_required_by_each_safe_command(string area, string action, string requiredOption)
    {
        var cliAssembly = Assembly.Load("CyberBank.LEdition.Cli");
        var parserType = cliAssembly.GetType("CyberBank.LEdition.Cli.LocalCliParser");
        Assert.NotNull(parserType);

        var parseMethod = parserType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(parseMethod);

        var result = parseMethod.Invoke(null, [new[] { area, action, "--project", "C:\\Assessments\\Local" }]);
        Assert.NotNull(result);

        var resultType = result.GetType();
        Assert.Equal(false, resultType.GetProperty("IsAccepted")?.GetValue(result));
        Assert.Contains(requiredOption, resultType.GetProperty("Message")?.GetValue(result)?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_rejects_a_remote_evidence_file_path()
    {
        var cliAssembly = Assembly.Load("CyberBank.LEdition.Cli");
        var parserType = cliAssembly.GetType("CyberBank.LEdition.Cli.LocalCliParser");
        Assert.NotNull(parserType);

        var parseMethod = parserType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(parseMethod);

        var result = parseMethod.Invoke(
            null,
            [new[] { "evidence", "import", "--project", "C:\\Assessments\\Local", "--file", "https://approved.example/evidence.json" }]);
        Assert.NotNull(result);

        var resultType = result.GetType();
        Assert.Equal(false, resultType.GetProperty("IsAccepted")?.GetValue(result));
        Assert.Contains("local", resultType.GetProperty("Message")?.GetValue(result)?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Help_explicitly_states_the_local_only_non_scanning_boundary()
    {
        var cliAssembly = Assembly.Load("CyberBank.LEdition.Cli");
        var helpType = cliAssembly.GetType("CyberBank.LEdition.Cli.LocalCliHelp");

        Assert.NotNull(helpType);

        var text = helpType.GetProperty("Text", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.ToString();

        Assert.NotNull(text);
        Assert.Contains("local project and evidence tool", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not a scanner or attack interface", text, StringComparison.OrdinalIgnoreCase);
    }
}
