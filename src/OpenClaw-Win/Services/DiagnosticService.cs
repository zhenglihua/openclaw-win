using System.Diagnostics;
using System.Net.Sockets;
using OpenClawWin.Models;

namespace OpenClawWin.Services;

public interface IDiagnosticService
{
    Task<DiagnosticResult> CheckNodeJsAsync();
    Task<DiagnosticResult> CheckNpmAsync();
    Task<DiagnosticResult> CheckOpenClawAsync();
    Task<DiagnosticResult> CheckPortAsync(int port);
    Task<List<DiagnosticResult>> RunFullDiagnosticAsync();
    DiagnosticResult CheckNodeJs();
    DiagnosticResult CheckOpenClaw();
}

public class DiagnosticService : IDiagnosticService
{
    public DiagnosticResult CheckNodeJs()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return new DiagnosticResult
                {
                    Name = "Node.js",
                    Status = DiagnosticStatus.NotInstalled,
                    Message = "Node.js 未安装",
                    Suggestion = "请安装 Node.js >= 22"
                };
            }

            var version = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode == 0 && !string.IsNullOrEmpty(version))
            {
                var versionNum = version.TrimStart('v');
                if (int.TryParse(versionNum.Split('.')[0], out int major) && major >= 22)
                {
                    return new DiagnosticResult
                    {
                        Name = "Node.js",
                        Status = DiagnosticStatus.OK,
                        Message = "Node.js 已安装",
                        Version = version
                    };
                }
                else
                {
                    return new DiagnosticResult
                    {
                        Name = "Node.js",
                        Status = DiagnosticStatus.Warning,
                        Message = $"Node.js 版本过低: {version}",
                        Version = version,
                        Suggestion = "请升级到 Node.js >= 22"
                    };
                }
            }

            return new DiagnosticResult
            {
                Name = "Node.js",
                Status = DiagnosticStatus.Error,
                Message = "Node.js 检查失败"
            };
        }
        catch
        {
            return new DiagnosticResult
            {
                Name = "Node.js",
                Status = DiagnosticStatus.NotInstalled,
                Message = "Node.js 未安装",
                Suggestion = "请安装 Node.js >= 22"
            };
        }
    }

    public async Task<DiagnosticResult> CheckNodeJsAsync()
    {
        return await Task.Run(() => CheckNodeJs());
    }

    public async Task<DiagnosticResult> CheckNpmAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "npm",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process == null)
                {
                    return new DiagnosticResult
                    {
                        Name = "npm",
                        Status = DiagnosticStatus.NotInstalled,
                        Message = "npm 未安装"
                    };
                }

                var version = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    return new DiagnosticResult
                    {
                        Name = "npm",
                        Status = DiagnosticStatus.OK,
                        Message = "npm 已安装",
                        Version = version
                    };
                }

                return new DiagnosticResult
                {
                    Name = "npm",
                    Status = DiagnosticStatus.Error,
                    Message = "npm 检查失败"
                };
            }
            catch
            {
                return new DiagnosticResult
                {
                    Name = "npm",
                    Status = DiagnosticStatus.NotInstalled,
                    Message = "npm 未安装"
                };
            }
        });
    }

    public DiagnosticResult CheckOpenClaw()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "openclaw",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return new DiagnosticResult
                {
                    Name = "OpenClaw",
                    Status = DiagnosticStatus.NotInstalled,
                    Message = "OpenClaw 未安装",
                    Suggestion = "请运行安装向导安装 OpenClaw"
                };
            }

            var version = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                return new DiagnosticResult
                {
                    Name = "OpenClaw",
                    Status = DiagnosticStatus.OK,
                    Message = "OpenClaw 已安装",
                    Version = version
                };
            }

            return new DiagnosticResult
            {
                Name = "OpenClaw",
                Status = DiagnosticStatus.Error,
                Message = "OpenClaw 检查失败"
            };
        }
        catch
        {
            return new DiagnosticResult
            {
                Name = "OpenClaw",
                Status = DiagnosticStatus.NotInstalled,
                Message = "OpenClaw 未安装",
                Suggestion = "请运行安装向导安装 OpenClaw"
            };
        }
    }

    public async Task<DiagnosticResult> CheckOpenClawAsync()
    {
        return await Task.Run(() => CheckOpenClaw());
    }

    public async Task<DiagnosticResult> CheckPortAsync(int port)
    {
        return await Task.Run(() =>
        {
            try
            {
                var listener = new TcpListener(System.Net.IPAddress.Loopback, port);
                listener.Start();
                listener.Stop();

                return new DiagnosticResult
                {
                    Name = $"端口 {port}",
                    Status = DiagnosticStatus.Warning,
                    Message = $"端口 {port} 可用",
                    Suggestion = "端口未被占用，可以使用"
                };
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
            {
                return new DiagnosticResult
                {
                    Name = $"端口 {port}",
                    Status = DiagnosticStatus.OK,
                    Message = $"端口 {port} 已被占用",
                    Suggestion = "Gateway 可能正在运行"
                };
            }
            catch
            {
                return new DiagnosticResult
                {
                    Name = $"端口 {port}",
                    Status = DiagnosticStatus.Error,
                    Message = "端口检查失败"
                };
            }
        });
    }

    public async Task<List<DiagnosticResult>> RunFullDiagnosticAsync()
    {
        var results = new List<DiagnosticResult>
        {
            await CheckNodeJsAsync(),
            await CheckNpmAsync(),
            await CheckOpenClawAsync(),
            await CheckPortAsync(18789)
        };

        return results;
    }
}
