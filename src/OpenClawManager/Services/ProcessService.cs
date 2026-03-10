using System.Diagnostics;

namespace OpenClawManager.Services;

public interface IProcessService
{
    bool IsProcessRunning(string processName);
    Process? GetProcess(string processName);
    Task<bool> StartProcessAsync(string fileName, string arguments, string workingDirectory);
    bool KillProcess(string processName);
    bool KillProcess(int processId);
}

public class ProcessService : IProcessService
{
    public bool IsProcessRunning(string processName)
    {
        try
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public Process? GetProcess(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            return processes.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> StartProcessAsync(string fileName, string arguments, string workingDirectory)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = Process.Start(psi);
            return process != null;
        }
        catch
        {
            return false;
        }
    }

    public bool KillProcess(string processName)
    {
        try
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Kill();
            }
            return processes.Length > 0;
        }
        catch
        {
            return false;
        }
    }

    public bool KillProcess(int processId)
    {
        try
        {
            var process = Process.GetProcessById(processId);
            process.Kill();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
