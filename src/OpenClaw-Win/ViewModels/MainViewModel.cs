using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenClawWin.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "OpenClaw 管理器";

    [ObservableProperty]
    private object? _currentView;
}
