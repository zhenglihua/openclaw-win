namespace OpenClawWin.Models;

public class DiagnosticResult
{
    public string Name { get; set; } = "";
    public DiagnosticStatus Status { get; set; }
    public string Message { get; set; } = "";
    public string? Version { get; set; }
    public string? Suggestion { get; set; }
}

public enum DiagnosticStatus
{
    OK,
    Warning,
    Error,
    NotInstalled
}
