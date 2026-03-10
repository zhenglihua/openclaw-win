# OpenClaw Manager 构建脚本

param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "开始构建 OpenClaw Manager..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 获取脚本目录
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

# 1. 清理旧构建
Write-Host "[1/5] 清理旧构建..." -ForegroundColor Yellow
$PublishDir = Join-Path $ProjectRoot "publish"
if (Test-Path $PublishDir) {
    Remove-Item -Path $PublishDir -Recurse -Force
}

# 2. 发布项目
Write-Host "[2/5] 发布项目..." -ForegroundColor Yellow
$ProjectPath = Join-Path $ProjectRoot "src\OpenClaw-Win\OpenClaw-Win.csproj"
dotnet publish $ProjectPath `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "发布失败!" -ForegroundColor Red
    exit 1
}

# 3. 复制NSSM（如果存在）
Write-Host "[3/5] 检查伴随文件..." -ForegroundColor Yellow
$NssmSource = Join-Path $ScriptDir "nssm.exe"
$NssmDest = Join-Path $PublishDir "nssm.exe"
if (Test-Path $NssmSource) {
    Copy-Item -Path $NssmSource -Destination $NssmDest -Force
    Write-Host "  - NSSM 已复制" -ForegroundColor Green
}

# 4. 复制配置文件
$ConfigSource = Join-Path $ProjectRoot "src\OpenClaw-Win\appsettings.json"
$ConfigDest = Join-Path $PublishDir "appsettings.json"
if (Test-Path $ConfigSource) {
    Copy-Item -Path $ConfigSource -Destination $ConfigDest -Force
    Write-Host "  - 配置文件已复制" -ForegroundColor Green
}

# 5. 打包为zip
Write-Host "[4/5] 打包..." -ForegroundColor Yellow
$ZipName = "OpenClaw-Win-v1.0.0-win-x64"
$ZipPath = Join-Path $ProjectRoot "$ZipName.zip"
if (Test-Path $ZipPath) {
    Remove-Item -Path $ZipPath -Force
}
Compress-Archive -Path "$PublishDir\*" -DestinationPath $ZipPath

# 6. 输出结果
Write-Host "[5/5] 完成!" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "发布目录: $PublishDir" -ForegroundColor Green
Write-Host "安装包: $ZipPath" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
