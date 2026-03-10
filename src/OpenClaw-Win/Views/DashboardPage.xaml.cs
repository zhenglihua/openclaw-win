using System.Windows;
using System.Windows.Controls;
using OpenClawWin.ViewModels;

namespace OpenClawWin.Views;

public partial class DashboardPage : Page
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage()
    {
        InitializeComponent();
        _viewModel = new DashboardViewModel();
        DataContext = _viewModel;

        Loaded += DashboardPage_Loaded;
    }

    private async void DashboardPage_Loaded(object sender, RoutedEventArgs e)
    {
        // 页面加载完成后异步初始化状态，避免阻塞UI
        await _viewModel.InitializeAsync();
    }
}
