using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenClawManager.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _title = "OpenClaw 管理器";

    [ObservableProperty]
    private object? _currentView;
}
