using System.Windows.Controls;
using OpenClawManager.ViewModels;

namespace OpenClawManager.Views;

public partial class InstallPage : Page
{
    private readonly InstallViewModel _viewModel;

    public InstallPage()
    {
        InitializeComponent();
        _viewModel = new InstallViewModel();
        DataContext = _viewModel;
    }

    private void PreviousStep_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.PreviousStep();
    }

    private void NextStep_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.NextStep();
    }

    private async void StartInstall_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        await _viewModel.InstallAsync();
    }

    private void Finish_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        _viewModel.SaveConfig();
    }
}
