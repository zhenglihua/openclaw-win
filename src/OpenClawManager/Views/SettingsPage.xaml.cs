using System.Windows.Controls;
using Microsoft.Win32;
using OpenClawManager.ViewModels;

namespace OpenClawManager.Views;

public partial class SettingsPage : Page
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage()
    {
        InitializeComponent();
        _viewModel = new SettingsViewModel();
        DataContext = _viewModel;
    }

    private void BrowseWorkspace_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "选择工作区目录"
        };

        if (dialog.ShowDialog() == true)
        {
            _viewModel.WorkspacePath = dialog.FolderName;
        }
    }

    private void ResetDefaults_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.ResetToDefaults();
    }

    private void SaveSettings_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.SaveSettings();
    }
}
