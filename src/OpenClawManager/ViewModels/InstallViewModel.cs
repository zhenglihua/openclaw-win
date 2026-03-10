using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenClawManager.Models;
using OpenClawManager.Services;

namespace OpenClawManager.ViewModels;

public partial class InstallViewModel : ViewModelBase
{
    private readonly IProcessService _processService;
    private readonly IDiagnosticService _diagnosticService;
    private readonly IServiceInstaller _serviceInstaller;

    [ObservableProperty]
    private int _currentStep = 1;

    [ObservableProperty]
    private string _installProgress = "等待开始...";

    [ObservableProperty]
    private double _installPercentage = 0;

    [ObservableProperty]
    private string _nodeVersion = "";

    [ObservableProperty]
    private string _openClawVersion = "";

    [ObservableProperty]
    private bool _isNodeInstalled = false;

    [ObservableProperty]
    private bool _isOpenClawInstalled = false;

    [ObservableProperty]
    private bool _isChecking = false;

    [ObservableProperty]
    private string _installLog = "";

    // 配置项
    [ObservableProperty]
    private string _selectedModel = "DeepSeek";

    [ObservableProperty]
    private string _apiKey = "";

    [ObservableProperty]
    private int _port = 18789;

    [ObservableProperty]
    private string _token = "";

    [ObservableProperty]
    private string _workspacePath = "";

    public bool CanGoBack => CurrentStep > 1;
    public bool CanGoNext => CurrentStep < 3;
    public bool CanStartInstall => CurrentStep == 2;
    public bool CanFinish => CurrentStep == 4;

    public Visibility IsStep1Visible => CurrentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsStep2Visible => CurrentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsStep3Visible => CurrentStep == 3 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility IsStep4Visible => CurrentStep == 4 ? Visibility.Visible : Visibility.Collapsed;

    public InstallViewModel()
    {
        _processService = new ProcessService();
        _diagnosticService = new DiagnosticService();
        _serviceInstaller = new NssmServiceInstaller();

        // 异步检查环境
        _ = CheckEnvironmentAsync();
    }

    private async Task CheckEnvironmentAsync()
    {
        IsChecking = true;

        // 检查Node.js
        var nodeResult = await _diagnosticService.CheckNodeJsAsync();
        IsNodeInstalled = nodeResult.Status == DiagnosticStatus.OK;
        NodeVersion = nodeResult.Version ?? "";

        // 检查OpenClaw
        var openclawResult = await _diagnosticService.CheckOpenClawAsync();
        IsOpenClawInstalled = openclawResult.Status == DiagnosticStatus.OK;
        OpenClawVersion = openclawResult.Version ?? "";

        IsChecking = false;
    }

    public void PreviousStep()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanStartInstall));
            OnPropertyChanged(nameof(CanFinish));
            OnPropertyChanged(nameof(IsStep1Visible));
            OnPropertyChanged(nameof(IsStep2Visible));
            OnPropertyChanged(nameof(IsStep3Visible));
            OnPropertyChanged(nameof(IsStep4Visible));
        }
    }

    public void NextStep()
    {
        if (CurrentStep < 3)
        {
            CurrentStep++;
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanStartInstall));
            OnPropertyChanged(nameof(CanFinish));
            OnPropertyChanged(nameof(IsStep1Visible));
            OnPropertyChanged(nameof(IsStep2Visible));
            OnPropertyChanged(nameof(IsStep3Visible));
            OnPropertyChanged(nameof(IsStep4Visible));
        }
    }

    [RelayCommand]
    public async Task InstallAsync()
    {
        CurrentStep = 2;
        OnPropertyChanged(nameof(IsStep2Visible));

        try
        {
            // 步骤1: 如果没有Node.js，提示用户安装
            if (!IsNodeInstalled)
            {
                InstallProgress = "请先安装 Node.js (>= 22)";
                InstallLog += $"[错误] Node.js 未安装，请先安装 Node.js >= 22\n";
                return;
            }

            InstallProgress = "正在安装 OpenClaw...";
            InstallPercentage = 20;
            InstallLog += "[信息] 开始安装 OpenClaw...\n";

            // 步骤2: 安装OpenClaw
            var result = await _processService.StartProcessAsync(
                "npm",
                "install -g openclaw@latest",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            );

            if (result)
            {
                InstallPercentage = 80;
                InstallLog += "[成功] OpenClaw 安装完成\n";

                // 验证安装
                var verifyResult = await _diagnosticService.CheckOpenClawAsync();
                if (verifyResult.Status == DiagnosticStatus.OK)
                {
                    IsOpenClawInstalled = true;
                    OpenClawVersion = verifyResult.Version ?? "";
                    InstallPercentage = 100;
                    InstallProgress = "安装完成！";
                    InstallLog += "[成功] 验证安装成功\n";

                    CurrentStep = 3;
                    OnPropertyChanged(nameof(CanGoBack));
                    OnPropertyChanged(nameof(CanGoNext));
                    OnPropertyChanged(nameof(CanStartInstall));
                    OnPropertyChanged(nameof(CanFinish));
                    OnPropertyChanged(nameof(IsStep1Visible));
                    OnPropertyChanged(nameof(IsStep2Visible));
                    OnPropertyChanged(nameof(IsStep3Visible));
                    OnPropertyChanged(nameof(IsStep4Visible));
                }
            }
            else
            {
                InstallProgress = "安装失败";
                InstallLog += "[错误] OpenClaw 安装失败\n";
            }
        }
        catch (Exception ex)
        {
            InstallProgress = "安装失败";
            InstallLog += $"[错误] {ex.Message}\n";
        }
    }

    public void SaveConfig()
    {
        var config = new AppConfig
        {
            GatewayPort = Port,
            WorkspacePath = WorkspacePath,
            AutoStartWithWindows = true,
            AutoCheckUpdates = true,
            LogLevel = "Information",
            MinimizeToTray = true,
            StartMinimized = false
        };

        var configPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OpenClawManager",
            "config.json"
        );

        var directory = System.IO.Path.GetDirectoryName(configPath);
        if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        System.IO.File.WriteAllText(configPath, json);

        InstallLog += $"[信息] 配置已保存到 {configPath}\n";
    }
}
