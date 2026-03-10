namespace OpenClawWin.Models;

public class AppConfig
{
    public int GatewayPort { get; set; } = 18789;
    public string WorkspacePath { get; set; } = "";
    public bool AutoStartWithWindows { get; set; } = true;
    public bool AutoCheckUpdates { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
    public bool MinimizeToTray { get; set; } = true;
    public bool StartMinimized { get; set; } = false;
}
