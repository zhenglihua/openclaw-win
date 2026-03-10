using System.Windows.Controls;
using OpenClawWin.ViewModels;

namespace OpenClawWin.Views;

public partial class StatusPage : Page
{
    private readonly StatusViewModel _viewModel;

    public StatusPage()
    {
        InitializeComponent();
        _viewModel = new StatusViewModel();
        DataContext = _viewModel;

        Loaded += (s, e) => _viewModel.StartMonitoring();
        Unloaded += (s, e) => _viewModel.StopMonitoring();
    }
}
