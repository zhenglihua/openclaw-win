using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenClawWin.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
}
