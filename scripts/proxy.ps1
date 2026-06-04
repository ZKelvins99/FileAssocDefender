# 为本项目当前 PowerShell 会话设置代理（不写入系统/用户环境变量）
# 用法：在项目根目录执行  . .\scripts\proxy.ps1

$Script:ProxyHost = "127.0.0.1"
$Script:HttpPort = 10809
$Script:SocksPort = 10808

# 可选：复制 proxy.local.ps1.example 为 proxy.local.ps1 覆盖端口（该文件已 gitignore）
$local = Join-Path $PSScriptRoot "proxy.local.ps1"
if (Test-Path $local) {
    . $local
}

$env:HTTP_PROXY = "http://${Script:ProxyHost}:${Script:HttpPort}"
$env:HTTPS_PROXY = "http://${Script:ProxyHost}:${Script:HttpPort}"
$env:ALL_PROXY = "socks5://${Script:ProxyHost}:${Script:SocksPort}"

Write-Host "==> 项目代理已启用（仅当前终端会话）" -ForegroundColor Cyan
Write-Host "    HTTP/HTTPS -> $($env:HTTP_PROXY)"
Write-Host "    ALL_PROXY  -> $($env:ALL_PROXY)"
