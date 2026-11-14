namespace CyberBank.LEdition.Cli;

public sealed record LocalCliParseResult(
    bool IsAccepted,
    string CommandName,
    string Message);
