namespace CyberBank.LEdition.Desktop.ViewModels;

public sealed class ThemePreference
{
    private bool isDark;

    public bool IsDark => isDark;

    public string CurrentThemeName => isDark ? "DARK" : "LIGHT";

    public string NextThemeActionLabel => isDark ? "Switch to light" : "Switch to dark";

    public void Toggle()
    {
        isDark = !isDark;
    }
}
