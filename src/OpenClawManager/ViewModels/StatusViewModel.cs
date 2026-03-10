using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ServiceProcess;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenClawManager.Models;
using OpenClawManager.Services;

namespace OpenClawManager.ViewModels;

public class LogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Message { get; set; } = "";
}

public partial class StatusViewModel : ViewModelBase
{
    private readonly IProcessService _processService;
    private readonly IDiagnosticService _diagnosticService;
    private readonly IServiceInstaller _serviceInstaller;
    private readonly DispatcherTimer _monitorTimer;

    [ObservableProperty]
    private bool _isGatewayRunning;

    [ObservableProperty]
    private bool _isPortInUse;

    [ObservableProperty]
    private bool _isServiceRunning;

    [ObservableProperty]
    private int _currentPort = 18789;

    [ObservableProperty]
    private string _gatewayUptime = "";

    [ObservableProperty]
    private int _connectedClients;

    [ObservableProperty]
    private ObservableCollection<LogEntry> _recentLogs = new();

    [ObservableProperty]
    private Brush _gatewayStatusColor = Brushes.Gray;

    [ObservableProperty]
    private string _gatewayStatusText = "未运行";

    [ObservableProperty]
    private Brush _portStatusColor = Brushes.Gray;

    [ObservableProperty]
    private string _portStatusText = "未检测";

    [ObservableProperty]
    private Brush _serviceStatusColor = Brushes.Gray;

    [ObservableProperty]
    private string _serviceStatusText = "未注册";

    public StatusViewModel()
    {
        _processService = new ProcessService();
        _diagnosticService = new DiagnosticService();
        _serviceInstaller = new NssmServiceInstaller();

        _monitorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _monitorTimer.Tick += async (s, e) => await RefreshStatusAsync();

        AddLog("状态监控已启动");
    }

    public void StartMonitoring()
    {
        _monitorTimer.Start();
        _ = RefreshStatusAsync();
    }

    public void StopMonitoring()
    {
        _monitorTimer.Stop();
    }

    private async Task RefreshStatusAsync()
    {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            // 检测进程状态
            IsGatewayRunning = _processService.IsProcessRunning("node");
            if (IsGatewayRunning)
            {
                GatewayStatusText = "运行中";
                GatewayStatusColor = Brushes.Green;
            }
            else
            {
                GatewayStatusText = "未运行";
                GatewayStatusColor = Brushes.Red;
            }

            // 检测端口状态
            var portResult = Task.Run(() => _diagnosticService.CheckPortAsync(CurrentPort)).Result;
            IsPortInUse = portResult.Status == DiagnosticStatus.OK;
            if (IsPortInUse)
            {
                PortStatusText = $"端口 {CurrentPort} 已占用";
                PortStatusColor = Brushes.Orange;
            }
            else
            {
                PortStatusText = $"端口 {CurrentPort} 可用";
                PortStatusColor = Brushes.Green;
            }

            // 检测服务状态
            if (_serviceInstaller.IsServiceInstalled("OpenClawGateway"))
            {
                var status = _serviceInstaller.GetServiceStatus("OpenClawGateway");
                IsServiceRunning = status == ServiceControllerStatus.Running;
                if (IsServiceRunning)
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
        });
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        AddLog("正在启动服务...");
        await _processService.StartProcessAsync(
            "cmd.exe",
            $"/c openclaw gateway run --port {CurrentPort}",
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        );
        AddLog("服务已启动");
        await RefreshStatusAsync();
    }

    [RelayCommand]
    private void Stop()
    {
        AddLog("正在停止服务...");
        _processService.KillProcess("node");
        AddLog("服务已停止");
        _ = RefreshStatusAsync();
    }

    [RelayCommand]
    private async Task RestartAsync()
    {
        AddLog("正在重启服务...");
        Stop();
        await Task.Delay(1000);
        await StartAsync();
    }

    [RelayCommand]
    private void OpenWebConsole()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = $"http://localhost:{CurrentPort}",
            UseShellExecute = true
        });
        AddLog("已打开Web控制台");
    }

    [RelayCommand]
    private async Task RefreshLogsAsync()
    {
        AddLog("刷新日志...");
        await RefreshStatusAsync();
    }

    [RelayCommand]
    private void ClearLogs()
    {
        RecentLogs.Clear();
        AddLog("日志已清空");
    }

    private void AddLog(string message)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            RecentLogs.Insert(0, new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = $"[{DateTime.Now:HH:mm:ss}] {message}"
            });

            // 限制日志数量
            while (RecentLogs.Count > 100)
            {
                RecentLogs.RemoveAt(RecentLogs.Count - 1);
            }
        });
    }
}
