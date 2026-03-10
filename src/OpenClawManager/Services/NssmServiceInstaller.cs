using System.ServiceProcess;
using System.Diagnostics;

namespace OpenClawManager.Services;

public interface IServiceInstaller
{
    Task<bool> InstallServiceAsync(string serviceName, string executablePath, string displayName);
    Task<bool> UninstallServiceAsync(string serviceName);
    Task<bool> StartServiceAsync(string serviceName);
    Task<bool> StopServiceAsync(string serviceName);
    bool IsServiceInstalled(string serviceName);
    ServiceControllerStatus? GetServiceStatus(string serviceName);
}

public class NssmServiceInstaller : IServiceInstaller
{
    public async Task<bool> InstallServiceAsync(string serviceName, string executablePath, string displayName)
    {
        return await Task.Run(() =>
        {
            try
            {
                // 使用sc命令创建服务
                var psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"create {serviceName} binPath= \"{executablePath}\" start= auto",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process?.WaitForExit();

                return process?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        });
    }

    public async Task<bool> UninstallServiceAsync(string serviceName)
    {
        return await Task.Run(() =>
        {
            try
            {
                // 先停止服务
                var stopPsi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"stop {serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var stopProcess = Process.Start(stopPsi);
                stopProcess?.WaitForExit();

                // 删除服务
                var deletePsi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"delete {serviceName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var deleteProcess = Process.Start(deletePsi);
                deleteProcess?.WaitForExit();

                return deleteProcess?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        });
    }

    public async Task<bool> StartServiceAsync(string serviceName)
    {
        return await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"start {serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process?.WaitForExit();

                return process?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        });
    }

    public async Task<bool> StopServiceAsync(string serviceName)
    {
        return await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"stop {serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                process?.WaitForExit();

                return process?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        });
    }

    public bool IsServiceInstalled(string serviceName)
    {
        try
        {
            var services = ServiceController.GetServices();
            return services.Any(s => s.ServiceName == serviceName);
        }
        catch
        {
            return false;
        }
    }

    public ServiceControllerStatus? GetServiceStatus(string serviceName)
    {
        try
        {
            var service = new ServiceController(serviceName);
            return service.Status;
        }
        catch
        {
            return null;
        }
    }
}
