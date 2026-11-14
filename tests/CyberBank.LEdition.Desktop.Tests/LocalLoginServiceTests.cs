using System.Reflection;
using Xunit;

namespace CyberBank.LEdition.Desktop.Tests;

public sealed class LocalLoginServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "cyberbank-l-edition-login-tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task Create_profile_stores_no_plaintext_passphrase_and_unlock_requires_the_matching_local_secret()
    {
        var assembly = Assembly.Load("CyberBank.LEdition.Desktop");
        var serviceType = assembly.GetType("CyberBank.LEdition.Desktop.Security.LocalLoginService");

        Assert.NotNull(serviceType);
        var service = Activator.CreateInstance(serviceType!, root);
        Assert.NotNull(service);

        var created = await InvokeAsync(service!, "CreateProfileAsync", "Ahmed", "Saffron!Lime2026");
        Assert.Equal(true, created.GetType().GetProperty("Succeeded")?.GetValue(created));
        Assert.Equal("Ahmed", created.GetType().GetProperty("DisplayName")?.GetValue(created));

        var profileContent = await File.ReadAllTextAsync(Path.Combine(root, "operator-profile.json"));
        Assert.DoesNotContain("Saffron!Lime2026", profileContent, StringComparison.Ordinal);

        var unlocked = await InvokeAsync(service!, "UnlockAsync", "Saffron!Lime2026");
        Assert.Equal(true, unlocked.GetType().GetProperty("Succeeded")?.GetValue(unlocked));

        var rejected = await InvokeAsync(service!, "UnlockAsync", "wrong-passphrase");
        Assert.Equal(false, rejected.GetType().GetProperty("Succeeded")?.GetValue(rejected));
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<object> InvokeAsync(object target, string methodName, params object[] parameters)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(method);

        var task = Assert.IsAssignableFrom<Task>(method!.Invoke(target, parameters));
        await task;

        return task.GetType().GetProperty("Result")?.GetValue(task)
            ?? throw new InvalidOperationException("The login operation returned no result.");
    }
}
