param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [ValidateSet("Build", "Test", "Publish", "Package", "All")]
    [string]$Target = "All",

    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

# 本项目脚本自动启用代理（仅当前 PowerShell 进程，不影响系统环境变量）
. (Join-Path $PSScriptRoot "proxy.ps1")

$Root = Split-Path -Parent $PSScriptRoot
$Artifacts = Join-Path $Root "artifacts"
$PublishDir = Join-Path $Artifacts "publish\$Runtime"
$PackageDir = Join-Path $Artifacts "package"

function Invoke-DotNet {
    param([string[]]$Arguments)
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet 命令失败: dotnet $($Arguments -join ' ')"
    }
}

Write-Host "==> FileAssocDefender build ($Target / $Configuration)" -ForegroundColor Cyan

if ($Target -in @("Build", "Test", "Publish", "Package", "All")) {
    Invoke-DotNet @("build", "$Root\FileAssocDefender\FileAssocDefender.csproj", "-c", $Configuration)
    Invoke-DotNet @("build", "$Root\FileAssocDefender.Tests\FileAssocDefender.Tests.csproj", "-c", $Configuration)
}

if ($Target -in @("Test", "All")) {
    Invoke-DotNet @("test", "$Root\FileAssocDefender.Tests\FileAssocDefender.Tests.csproj", "-c", $Configuration, "--no-build")
}

if ($Target -in @("Publish", "Package", "All")) {
    if (Test-Path $PublishDir) {
        Remove-Item $PublishDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null

    Invoke-DotNet @(
        "publish", "$Root\FileAssocDefender\FileAssocDefender.csproj",
        "-c", $Configuration,
        "-r", $Runtime,
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:PublishReadyToRun=true",
        "-o", $PublishDir
    )

    $exePath = Join-Path $PublishDir "FileAssocDefender.exe"
    if (-not (Test-Path $exePath)) {
        throw "未找到发布产物: $exePath"
    }

    Write-Host "==> EXE 已发布: $exePath" -ForegroundColor Green
}

if ($Target -in @("Package", "All")) {
    if (-not (Test-Path (Join-Path $PublishDir "FileAssocDefender.exe"))) {
        throw "请先执行 Publish，或运行 -Target All"
    }

    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw "未找到 dotnet"
    }

    New-Item -ItemType Directory -Path $PackageDir -Force | Out-Null

    Invoke-DotNet @(
        "build", "$Root\Installer\FileAssocDefender.Installer.wixproj",
        "-c", $Configuration
    )

    $msi = Get-ChildItem -Path (Join-Path $Root "Installer\bin\$Configuration") -Filter "*.msi" -Recurse |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $msi) {
        throw "未找到 MSI 产物"
    }

    Copy-Item $msi.FullName (Join-Path $PackageDir $msi.Name) -Force
    Write-Host "==> MSI 已生成: $($msi.FullName)" -ForegroundColor Green
    Write-Host "==> MSI 已复制: $(Join-Path $PackageDir $msi.Name)" -ForegroundColor Green
}

Write-Host "==> 完成" -ForegroundColor Cyan
