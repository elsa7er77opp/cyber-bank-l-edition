using System.Reflection;
using Xunit;

namespace CyberBank.LEdition.Desktop.Tests;

public sealed class ThemePreferenceTests
{
    [Fact]
    public void Toggle_moves_from_light_to_dark_and_exposes_the_next_action_label()
    {
        var desktopAssembly = Assembly.Load("CyberBank.LEdition.Desktop");
        var preferenceType = desktopAssembly.GetType("CyberBank.LEdition.Desktop.ViewModels.ThemePreference");

        Assert.NotNull(preferenceType);

        var preference = Activator.CreateInstance(preferenceType);
        var toggleMethod = preferenceType.GetMethod("Toggle", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(preference);
        Assert.NotNull(toggleMethod);
        Assert.Equal("LIGHT", preferenceType.GetProperty("CurrentThemeName")?.GetValue(preference));
        Assert.Equal("Switch to dark", preferenceType.GetProperty("NextThemeActionLabel")?.GetValue(preference));

        toggleMethod.Invoke(preference, null);

        Assert.Equal("DARK", preferenceType.GetProperty("CurrentThemeName")?.GetValue(preference));
        Assert.Equal("Switch to light", preferenceType.GetProperty("NextThemeActionLabel")?.GetValue(preference));
    }
}
