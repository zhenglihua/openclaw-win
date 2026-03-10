namespace OpenClawManager.Models;

public class OpenClawStatus
{
    public bool IsInstalled { get; set; }
    public bool IsGatewayRunning { get; set; }
    public bool IsPortInUse { get; set; }
    public bool IsServiceRegistered { get; set; }
    public bool IsServiceRunning { get; set; }
    public string? Version { get; set; }
    public int Port { get; set; } = 18789;
    public DateTime? StartTime { get; set; }
    public TimeSpan Uptime => StartTime.HasValue ? DateTime.Now - StartTime.Value : TimeSpan.Zero;
}
