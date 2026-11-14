using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Threading;
using CyberBank.LEdition.Desktop.Security;
using CyberBank.LEdition.Desktop.ViewModels;

namespace CyberBank.LEdition.Desktop;

public partial class LoginWindow : Window
{
    private readonly ThemePreference themePreference = new();
    private readonly LocalLoginService loginService = new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CyberBank",
        "L Edition",
        "Identity"));

    public LoginWindow()
    {
        InitializeComponent();
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        ConfigureMode();
    }

    private void ConfigureMode()
    {
        var unlockMode = loginService.ProfileExists();
        DisplayNameSection.IsVisible = !unlockMode;
        ConfirmPassphraseSection.IsVisible = !unlockMode;
        LoginTitle.Text = unlockMode ? "Unlock your local workspace" : "Create your local operator profile";
        LoginDescription.Text = unlockMode
            ? "Enter your local passphrase to open case records on this device."
            : "Set a display name and a local passphrase. The passphrase is not saved as text.";
        LoginButton.Content = unlockMode ? "Unlock local workspace" : "Create local profile";
        LoginMessageText.Text = unlockMode ? "This device already has a local L EDITION profile." : string.Empty;
    }

    private async void LoginButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoginButton.IsEnabled = false;
        try
        {
            LocalLoginResult result;
            if (loginService.ProfileExists())
            {
                result = await loginService.UnlockAsync(PassphraseInput.Text ?? string.Empty);
            }
            else
            {
                if (!string.Equals(PassphraseInput.Text, ConfirmPassphraseInput.Text, StringComparison.Ordinal))
                {
                    LoginMessageText.Text = "The local passphrase confirmation does not match.";
                    return;
                }

                result = await loginService.CreateProfileAsync(
                    DisplayNameInput.Text ?? string.Empty,
                    PassphraseInput.Text ?? string.Empty);
            }

            if (!result.Succeeded)
            {
                LoginMessageText.Text = result.Message;
                return;
            }

            LoginMessageText.Text = result.Message;
            var workspace = new MainWindow
            {
                Width = 1400,
                Height = 820,
                WindowState = WindowState.Normal,
            };
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = workspace;
            }

            workspace.Closed += (_, _) => Close();
            workspace.Opened += (_, _) => DispatcherTimer.RunOnce(() =>
            {
                workspace.WindowState = WindowState.Normal;
                workspace.Activate();
            }, TimeSpan.FromMilliseconds(180));
            Hide();
            workspace.Show();
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private void ToggleTheme_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        themePreference.Toggle();
        Avalonia.Application.Current!.RequestedThemeVariant = themePreference.IsDark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
        ThemeToggleButton.Content = themePreference.NextThemeActionLabel;
    }
}
