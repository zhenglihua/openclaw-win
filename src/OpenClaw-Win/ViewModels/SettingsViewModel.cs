using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenClawWin.Models;

namespace OpenClawWin.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly string _configPath;

    [ObservableProperty]
    private int _gatewayPort = 18789;

    [ObservableProperty]
    private string _workspacePath = "";

    [ObservableProperty]
    private bool _autoStartWithWindows = true;

    [ObservableProperty]
    private bool _autoCheckUpdates = true;

    [ObservableProperty]
    private string _logLevel = "Information";

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private bool _startMinimized = false;

    public SettingsViewModel()
    {
        _configPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OpenClawWin",
            "config.json"
        );

        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (System.IO.File.Exists(_configPath))
            {
                var json = System.IO.File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json);
                if (config != null)
                {
                    GatewayPort = config.GatewayPort;
                    WorkspacePath = config.WorkspacePath;
                    AutoStartWithWindows = config.AutoStartWithWindows;
                    AutoCheckUpdates = config.AutoCheckUpdates;
                    LogLevel = config.LogLevel;
                    MinimizeToTray = config.MinimizeToTray;
                    StartMinimized = config.StartMinimized;
                }
            }
        }
        catch
        {
            // 使用默认设置
        }
    }

    [RelayCommand]
    public void SaveSettings()
    {
        try
        {
            var config = new AppConfig
            {
                GatewayPort = GatewayPort,
                WorkspacePath = WorkspacePath,
                AutoStartWithWindows = AutoStartWithWindows,
                AutoCheckUpdates = AutoCheckUpdates,
                LogLevel = LogLevel,
                MinimizeToTray = MinimizeToTray,
                StartMinimized = StartMinimized
            };

            var directory = System.IO.Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(_configPath, json);
        }
        catch
        {
            // 保存失败
        }
    }

    [RelayCommand]
    public void ResetToDefaults()
    {
        GatewayPort = 18789;
        WorkspacePath = "";
        AutoStartWithWindows = true;
        AutoCheckUpdates = true;
        LogLevel = "Information";
        MinimizeToTray = true;
        StartMinimized = false;
    }
}
