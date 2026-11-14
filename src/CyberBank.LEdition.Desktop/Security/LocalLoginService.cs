using System.Security.Cryptography;
using System.Text.Json;

namespace CyberBank.LEdition.Desktop.Security;

public sealed class LocalLoginService
{
    private const int Iterations = 210_000;
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private readonly string profilePath;

    public LocalLoginService(string profileRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileRoot);
        profilePath = Path.Combine(profileRoot, "operator-profile.json");
    }

    public async Task<LocalLoginResult> CreateProfileAsync(string displayName, string passphrase)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return new LocalLoginResult(false, null, "A local display name is required.");
        }

        if (string.IsNullOrWhiteSpace(passphrase) || passphrase.Length < 12)
        {
            return new LocalLoginResult(false, null, "Use a local passphrase with at least 12 characters.");
        }

        var salt = RandomNumberGenerator.GetBytes(SaltLength);
        var hash = Derive(passphrase, salt);
        var profile = new StoredLocalProfile(displayName.Trim(), Convert.ToBase64String(salt), Convert.ToBase64String(hash));

        Directory.CreateDirectory(Path.GetDirectoryName(profilePath)!);
        await File.WriteAllTextAsync(profilePath, JsonSerializer.Serialize(profile, JsonOptions));

        return new LocalLoginResult(true, profile.DisplayName, "Local profile created.");
    }

    public async Task<LocalLoginResult> UnlockAsync(string passphrase)
    {
        if (!File.Exists(profilePath))
        {
            return new LocalLoginResult(false, null, "Create a local profile first.");
        }

        var profile = JsonSerializer.Deserialize<StoredLocalProfile>(await File.ReadAllTextAsync(profilePath), JsonOptions);
        if (profile is null)
        {
            return new LocalLoginResult(false, null, "The local profile could not be read.");
        }

        var salt = Convert.FromBase64String(profile.Salt);
        var expectedHash = Convert.FromBase64String(profile.Hash);
        var candidateHash = Derive(passphrase ?? string.Empty, salt);
        var succeeded = CryptographicOperations.FixedTimeEquals(expectedHash, candidateHash);

        return succeeded
            ? new LocalLoginResult(true, profile.DisplayName, "Local workspace unlocked.")
            : new LocalLoginResult(false, null, "The local passphrase did not match.");
    }

    public bool ProfileExists() => File.Exists(profilePath);

    private static byte[] Derive(string passphrase, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(passphrase, salt, Iterations, HashAlgorithmName.SHA512, HashLength);

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private sealed record StoredLocalProfile(string DisplayName, string Salt, string Hash);
}
