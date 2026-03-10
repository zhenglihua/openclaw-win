using System.Diagnostics;
using System.ServiceProcess;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenClawWin.Models;
using OpenClawWin.Services;

namespace OpenClawWin.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IProcessService _processService;
    private readonly IDiagnosticService _diagnosticService;
    private readonly IServiceInstaller _serviceInstaller;

    [ObservableProperty]
    private string _openClawStatusText = "未安装";

    [ObservableProperty]
    private Brush _openClawStatusColor = Brushes.Gray;

    [ObservableProperty]
    private string _gatewayStatusText = "未运行";

    [ObservableProperty]
    private Brush _gatewayStatusColor = Brushes.Gray;

    [ObservableProperty]
    private string _portStatusText = "未检测";

    [ObservableProperty]
    private Brush _portStatusColor = Brushes.Gray;

    [ObservableProperty]
    private string _serviceStatusText = "未注册";

    [ObservableProperty]
    private Brush _serviceStatusColor = Brushes.Gray;

    public DashboardViewModel()
    {
        _processService = new ProcessService();
        _diagnosticService = new DiagnosticService();
        _serviceInstaller = new NssmServiceInstaller();

        // 不要在构造函数中同步调用耗时操作
        // RefreshStatus() 会在页面 Loaded 事件中异步调用
    }

    public async Task InitializeAsync()
    {
        await RefreshStatusAsync();
    }

    [RelayCommand]
    private async Task RefreshStatusAsync()
    {
        // 检查OpenClaw安装状态
        var openClawResult = _diagnosticService.CheckOpenClaw();
        if (openClawResult.Status == DiagnosticStatus.OK)
        {
            OpenClawStatusText = $"v{openClawResult.Version}";
            OpenClawStatusColor = Brushes.Green;
        }
        else
        {
            OpenClawStatusText = "未安装";
            OpenClawStatusColor = Brushes.Orange;
        }

        // 检查Gateway进程
        if (_processService.IsProcessRunning("node"))
        {
            GatewayStatusText = "运行中";
            GatewayStatusColor = Brushes.Green;
        }
        else
        {
            GatewayStatusText = "未运行";
            GatewayStatusColor = Brushes.Red;
        }

        // 检查端口
        var portResult = await _diagnosticService.CheckPortAsync(18789);
        if (portResult.Status == DiagnosticStatus.OK)
        {
            PortStatusText = "18789 (已占用)";
            PortStatusColor = Brushes.Orange;
        }
        else
        {
            PortStatusText = "18789 (可用)";
            PortStatusColor = Brushes.Green;
        }

        // 检查Windows服务
        if (_serviceInstaller.IsServiceInstalled("OpenClawGateway"))
        {
            var status = _serviceInstaller.GetServiceStatus("OpenClawGateway");
            if (status == ServiceControllerStatus.Running)
            {
                ServiceStatusText = "运行中";
                ServiceStatusColor = Brushes.Green;
            }
            else
            {
                ServiceStatusText = "已停止";
                ServiceStatusColor = Brushes.Red;
            }
        }
        else
        {
            ServiceStatusText = "未注册";
            ServiceStatusColor = Brushes.Gray;
        }
    }

    [RelayCommand]
    private async Task StartServiceAsync()
    {
        await _processService.StartProcessAsync("cmd.exe", "/c openclaw gateway run --port 18789", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        await RefreshStatusAsync();
    }

    [RelayCommand]
    private async Task StopServiceAsync()
    {
        _processService.KillProcess("node");
        await RefreshStatusAsync();
    }

    [RelayCommand]
    private async Task RestartServiceAsync()
    {
        _processService.KillProcess("node");
        await Task.Delay(1000);
        await StartServiceAsync();
    }

    [RelayCommand]
    private void OpenWebUI()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "http://localhost:18789",
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private async Task RunDiagnosticAsync()
    {
        await RefreshStatusAsync();
    }
}
