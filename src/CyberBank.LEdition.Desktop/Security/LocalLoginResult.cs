namespace CyberBank.LEdition.Desktop.Security;

public sealed record LocalLoginResult(bool Succeeded, string? DisplayName, string Message);
