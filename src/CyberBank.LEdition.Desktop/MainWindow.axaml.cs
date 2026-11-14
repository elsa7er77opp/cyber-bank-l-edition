using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using System.Text.Json;
using CyberBank.LEdition.Desktop.ViewModels;
using CyberBank.LEdition.Domain;
using CyberBank.LEdition.Infrastructure;

namespace CyberBank.LEdition.Desktop;

public partial class MainWindow : Window
{
    private readonly LocalEvidenceImporter evidenceImporter = new();
    private readonly LocalReportExporter reportExporter = new();
    private readonly ThemePreference themePreference = new();
    private LocalProjectStore? projectStore;
    private string? projectRoot;

    public MainWindow()
    {
        InitializeComponent();
        Avalonia.Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        DataContext = new WorkstationViewModel();
        Opened += async (_, _) => await LoadMostRecentProjectAsync();
    }

    private void ToggleTheme_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        themePreference.Toggle();
        Avalonia.Application.Current!.RequestedThemeVariant = themePreference.IsDark
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
        ThemeToggleButton.Content = themePreference.NextThemeActionLabel;
    }

    private async void CreateEngagement_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkstationViewModel viewModel)
        {
            return;
        }

        try
        {
            viewModel.CreateAuthorizedEngagement(
                EngagementNameInput.Text ?? string.Empty,
                AuthorizationReferenceInput.Text ?? string.Empty,
                InScopeInput.Text ?? string.Empty,
                OutOfScopeInput.Text ?? string.Empty);
            projectRoot = CreateLocalProjectRoot(viewModel.ActiveEngagementName);
            projectStore = new LocalProjectStore(projectRoot);
            await projectStore.SaveAsync(viewModel.CurrentEngagement!);
            GateErrorText.Text = "Declared scope is ready and saved locally. You can now import selected local evidence.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.LightSkyBlue;
        }
        catch (ArgumentException exception)
        {
            GateErrorText.Text = exception.Message;
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
        }
    }

    private async void ImportEvidence_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkstationViewModel viewModel || viewModel.CurrentEngagement is null || string.IsNullOrWhiteSpace(projectRoot))
        {
            GateErrorText.Text = "Create a valid authorized engagement before selecting local evidence.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select local evidence to copy into this authorized engagement",
            AllowMultiple = false
        });

        var selectedFile = files.FirstOrDefault();
        var localPath = selectedFile?.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(localPath))
        {
            return;
        }

        try
        {
            var evidence = await evidenceImporter.ImportAsync(viewModel.CurrentEngagement, localPath, projectRoot);
            viewModel.RegisterEvidence(evidence);
            await PersistProjectAsync(viewModel);
            GateErrorText.Text = "Local evidence copied and hashed. No content was executed.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.LightSkyBlue;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            GateErrorText.Text = exception.Message;
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
        }
    }

    private async void CreateAsset_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkstationViewModel viewModel || viewModel.CurrentEngagement is null)
        {
            GateErrorText.Text = "Create a valid authorized engagement before adding a declared asset.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
            return;
        }

        try
        {
            var asset = AssetRecord.Create(
                viewModel.CurrentEngagement,
                AssetLabelInput.Text ?? string.Empty,
                AssetCategoryInput.Text ?? string.Empty,
                AssetScopeReferenceInput.Text ?? string.Empty);
            viewModel.RegisterAsset(asset);
            await PersistProjectAsync(viewModel);
            GateErrorText.Text = "Declared asset recorded against an in-scope boundary.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.LightSkyBlue;
        }
        catch (ArgumentException exception)
        {
            GateErrorText.Text = exception.Message;
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
        }
    }

    private async void CreateFinding_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkstationViewModel viewModel || viewModel.CurrentEngagement is null)
        {
            GateErrorText.Text = "Create a valid authorized engagement before adding a finding draft.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
            return;
        }

        try
        {
            var finding = Finding.Create(
                FindingTitleInput.Text,
                FindingSeverityInput.Text ?? "Medium",
                FindingImpactInput.Text ?? string.Empty,
                FindingRemediationInput.Text ?? string.Empty);
            if (!string.IsNullOrWhiteSpace(FindingEvidenceReferenceInput.Text))
            {
                finding.LinkEvidence(FindingEvidenceReferenceInput.Text);
            }

            viewModel.RegisterFinding(finding);
            await PersistProjectAsync(viewModel);
            GateErrorText.Text = "Assessor-authored finding saved in this local session.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.LightSkyBlue;
        }
        catch (ArgumentException exception)
        {
            GateErrorText.Text = exception.Message;
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
        }
    }

    private async void ExportReport_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not WorkstationViewModel viewModel || viewModel.CurrentEngagement is null || string.IsNullOrWhiteSpace(projectRoot))
        {
            GateErrorText.Text = "Create and save a valid authorized engagement before exporting a local report.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
            return;
        }

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose a local folder for the report snapshot",
            AllowMultiple = false
        });
        var outputDirectory = folders.FirstOrDefault()?.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            return;
        }

        try
        {
            var snapshot = await reportExporter.ExportAsync(
                viewModel.CurrentEngagement,
                viewModel.Assets,
                viewModel.EvidenceItems,
                viewModel.Findings,
                projectRoot,
                outputDirectory);
            viewModel.RegisterReportExport(snapshot.MarkdownPath);
            await PersistProjectAsync(viewModel);
            GateErrorText.Text = $"Local JSON and Markdown report exported. SHA-256: {snapshot.Sha256}";
            GateErrorText.Foreground = Avalonia.Media.Brushes.LightSkyBlue;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            GateErrorText.Text = exception.Message;
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
        }
    }

    private static string CreateLocalProjectRoot(string engagementName)
    {
        var safeName = string.Concat(engagementName
            .Select(character => char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-'))
            .Trim('-');
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "authorized-engagement";
        }

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CyberBank",
            "L Edition",
            "Projects",
            $"{safeName}-{Guid.NewGuid():N}");
    }

    private async Task PersistProjectAsync(WorkstationViewModel viewModel)
    {
        if (projectStore is null || viewModel.CurrentEngagement is null)
        {
            return;
        }

        await projectStore.SaveProjectAsync(
            viewModel.CurrentEngagement,
            viewModel.Assets,
            viewModel.EvidenceItems,
            viewModel.Findings);
    }

    private async Task LoadMostRecentProjectAsync()
    {
        if (DataContext is not WorkstationViewModel viewModel)
        {
            return;
        }

        var projectsRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CyberBank",
            "L Edition",
            "Projects");
        if (!Directory.Exists(projectsRoot))
        {
            return;
        }

        var latestManifest = Directory.EnumerateFiles(
                projectsRoot,
                "l-edition.project.json",
                SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(latestManifest))
        {
            return;
        }

        try
        {
            projectRoot = Path.GetDirectoryName(latestManifest);
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                return;
            }

            projectStore = new LocalProjectStore(projectRoot);
            var snapshot = await projectStore.LoadProjectAsync();
            viewModel.RestoreProject(
                snapshot.Engagement,
                snapshot.Assets,
                snapshot.Evidence,
                snapshot.Findings);
            GateErrorText.Text = "Most recent local project reopened. No evidence content was executed.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.LightSkyBlue;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            GateErrorText.Text = "The most recent local project could not be reopened.";
            GateErrorText.Foreground = Avalonia.Media.Brushes.Peru;
        }
    }
}
