# OpenClaw Windows管理报告

##工具 研究 研究日期

2026-03-04

## 研究问题

用户希望开发一个OpenClaw的Windows管理工具，需要实现以下功能：
1. 下载编译完成的OpenClaw安装包
2. 在Windows环境安装OpenClaw
3. 诊断、修复OpenClaw相关依赖
4. 监控OpenClaw服务状态
5. 支持手动启动
6. 采用WPF技术开发
7. 功能可参考OneClaw（一键安装包）

## 发现摘要

OpenClaw是一个开源的本地AI Agent框架，基于Node.js运行（需要Node.js >= 22），默认端口18789。OpenClaw的安装流程相对复杂，涉及Node.js环境配置、npm安装、守护进程安装、模型API配置等多个步骤。现有类似工具"OneClaw"提供了图形化的一键安装方案。WPF技术可以通过ServiceController类实现Windows服务管理，结合PowerShell可完成依赖诊断和修复功能。

## 相关文件清单

| 文件路径 | 作用说明 | 关键行号 |
|----------|----------|----------|
| .spec-workflow/templates/*.md | 项目模板文件 | - |
| - | - | - |

**注：项目为新建，目前无业务代码**

## OpenClaw核心知识

### 1. OpenClaw简介

OpenClaw（社区称"大龙虾"）是一个开源的本地AI Agent框架，具备系统级操作权限，可将自然语言指令转化为文件操作、程序控制、网络请求等行为。

- **官网**: https://openclaw.ai/
- **GitHub**: https://github.com/openclaw/openclaw
- **默认端口**: 18789

### 2. OpenClaw安装流程

**前置条件**:
- Node.js >= 22 (必需)
- Windows 10 21H2+ 或 Windows 11
- 建议内存 4GB+（推荐 8GB+）
- 管理员权限

**安装步骤**:

```powershell
# 1. 全局安装 OpenClaw
npm install -g openclaw@latest

# 2. 运行向导并安装系统服务（daemon）
openclaw onboard --install-daemon

# 3. 启动网关（如需手动运行）
openclaw gateway run --port 18789
```

**配置项**:
- 模型API（DeepSeek、GLM-4.7、Qwen等）
- 工作区目录（默认 C:\Users\<用户名>\.openclaw）
- Gateway认证token

### 3. OpenClaw核心命令

| 命令 | 说明 |
|------|------|
| `openclaw --version` | 查看版本 |
| `openclaw onboard --install-daemon` | 安装守护进程 |
| `openclaw gateway run --port 18789` | 启动网关 |
| `openclaw gateway status` | 查看网关状态 |
| `openclaw plugins install <plugin>` | 安装插件 |

### 4. OpenClaw服务架构

OpenClaw采用客户端-网关架构：
- **Gateway**: Web服务，默认监听 18789 端口
- **Daemon**: 后台服务，负责进程管理
- **工作区**: `~/.openclaw/` 目录存放配置和数据

## 现有类似工具分析

### OneClaw

OneClaw是社区开发的一键安装包，特点：
- 自动配置环境和依赖
- 无需手动安装Node.js、Git
- 内置简洁的图形界面
- 支持开机启动
- 针对国内网络优化

**功能特性**:
- 双击安装包即可安装
- 配置模型API信息
- 自动验证API可用性
- 提供Web UI访问入口

## WPF开发技术分析

### 1. WPF与Windows服务管理

WPF可通过以下方式管理Windows服务：

```csharp
// 使用 ServiceController 类
using System.ServiceProcess;

var service = new ServiceController("ServiceName");
if (service.Status == ServiceControllerStatus.Running)
{
    // 服务运行中
}

// 启动/停止服务
service.Start();
service.Stop();
```

### 2. WPF进程管理

```csharp
// 启动外部进程
var process = new Process();
process.StartInfo.FileName = "cmd.exe";
process.StartInfo.Arguments = "/c npm install -g openclaw";
process.Start();

// 检查进程是否存在
Process[] processes = Process.GetProcessesByName("node");
```

### 3. PowerShell集成

WPF可以通过PowerShell执行复杂的管理任务：

```csharp
var psi = new ProcessStartInfo();
psi.FileName = "powershell.exe";
psi.Arguments = "-Command \"Get-Process node\"";
psi.RedirectStandardOutput = true;
var process = Process.Start(psi);
```

### 4. WPF依赖检测

检测Node.js安装状态：

```powershell
# 检查Node.js版本
node --version

# 检查npm版本
npm --version

# 检查OpenClaw安装状态
openclaw --version
```

### 5. WPF端口监控

检测18789端口是否被占用：

```powershell
# 检查端口占用
netstat -ano | findstr :18789
```

## 功能模块设计建议

### 1. 安装管理模块

- **下载安装包**: 从GitHub Releases或官方源下载预编译包
- **环境检测**: 检测Node.js、npm是否已安装及版本
- **自动安装**: 一键安装OpenClaw
- **安装向导**: 引导配置模型API、token等

### 2. 依赖诊断模块

| 检测项 | 检测方法 | 修复方案 |
|--------|----------|----------|
| Node.js | `node --version` | 提示下载安装 |
| npm | `npm --version` | 随Node.js安装 |
| OpenClaw | `openclaw --version` | 执行npm install |
| 端口18789 | `netstat -ano` | 提示释放端口或换端口 |

### 3. 服务状态监控模块

- **进程监控**: 检测node进程是否运行
- **端口监控**: 检测18789端口状态
- **Web UI可访问性**: HTTP请求检测
- **实时状态面板**: 红/绿指示灯显示

### 4. 服务控制模块

- **启动服务**: `openclaw gateway run --port 18789`
- **停止服务**: 终止node进程
- **重启服务**: 先停后启

### 5. 日志管理模块

- **日志查看**: 读取 `~/.openclaw/logs/` 目录
- **日志分析**: 错误关键字过滤
- **日志导出**: 诊断报告生成

## 架构设计建议

### 技术栈

| 组件 | 技术选择 |
|------|----------|
| 框架 | .NET 8 + WPF |
| 语言 | C# 12 |
| UI模式 | MVVM |
| 日志 | Serilog |
| HTTP客户端 | HttpClient |

### 项目结构

```
OpenClawManager/
├── OpenClawManager.sln
├── src/
│   └── OpenClawManager/
│       ├── App.xaml
│       ├── MainWindow.xaml
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   ├── InstallViewModel.cs
│       │   ├── StatusViewModel.cs
│       │   └── SettingsViewModel.cs
│       ├── Models/
│       │   ├── OpenClawStatus.cs
│       │   ├── InstallConfig.cs
│       │   └── DiagnosticResult.cs
│       ├── Services/
│       │   ├── OpenClawService.cs
│       │   ├── DiagnosticService.cs
│       │   ├── ProcessService.cs
│       │   └── PowerShellService.cs
│       └── Views/
│           ├── InstallView.xaml
│           ├── StatusView.xaml
│           └── SettingsView.xaml
└── README.md
```

### 核心类设计

**OpenClawService**
- `InstallAsync()`: 安装OpenClaw
- `StartGatewayAsync()`: 启动网关
- `StopGatewayAsync()`: 停止网关
- `GetStatusAsync()`: 获取状态

**DiagnosticService**
- `CheckNodeJsAsync()`: 检测Node.js
- `CheckNpmAsync()`: 检测npm
- `CheckOpenClawAsync()`: 检测OpenClaw
- `CheckPortAsync(int port)`: 检测端口
- `RunFullDiagnosticAsync()`: 完整诊断

**ProcessService**
- `IsProcessRunning(string name)`: 检查进程
- `StartProcessAsync(string file, string args)`: 启动进程
- `KillProcess(string name)`: 终止进程

## 潜在风险和边缘情况

1. **管理员权限**: 部分操作需要管理员权限，需提示用户以管理员运行
2. **网络问题**: 下载安装包可能失败，需提供重试机制
3. **版本兼容性**: Node.js版本检测需兼容多个版本
4. **端口冲突**: 18789端口可能被占用，需提供端口配置
5. **进程僵尸**: node进程异常退出需检测和处理
6. **多实例**: 避免重复启动多个Gateway实例

## 开放问题

1. **安装源**: 从哪里下载预编译的OpenClaw安装包？（npm vs exe）
2. **服务注册**: 是否需要将OpenClaw注册为Windows服务？
3. **自动更新**: 是否需要支持自动检查更新？
4. **多语言**: 是否需要支持国际化？

## 参考资料

- OpenClaw官网: https://openclaw.ai/
- OpenClaw GitHub: https://github.com/openclaw/openclaw
- OpenClaw中文社区: https://clawd.org.cn/start/getting-started
- WPF文档: https://learn.microsoft.com/zh-cn/dotnet/wpf/
- .NET ServiceController: https://learn.microsoft.com/zh-CN/dotnet/api/system.serviceprocess.servicecontroller
