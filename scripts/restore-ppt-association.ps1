
# 紧急恢复 .ppt / .pptx 默认打开方式
# 1) 右键 PowerShell -> 以管理员身份运行
# 2) Set-Location 'd:\Code_zkelvins\FileAssocDefender\scripts'
# 3) .\restore-ppt-association.ps1
# 若 COM 失败，脚本会打开「默认应用」设置页，请手动选择 WPS 演示 或 PowerPoint。
# Restore .ppt / .pptx default app. Run PowerShell as Administrator when possible.

$ErrorActionPreference = "Continue"

function Try-ComSetDefault {
    param([string]$ProgId, [string]$Extension)
    try {
        $clsid = [Guid]"debd7aa0-0eae-4b57-8306-6914f363fb73"
        $type = [Type]::GetTypeFromCLSID($clsid)
        if ($null -eq $type) { return $false }
        $reg = [Activator]::CreateInstance($type)
        $reg.SetAppAsDefault($ProgId, $Extension, 0)
        return $true
    } catch {
        Write-Warning "COM failed for $Extension : $($_.Exception.Message)"
        return $false
    }
}

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
Write-Host "Admin: $isAdmin"

$okPptx = Try-ComSetDefault "PowerPoint.Show.12" ".pptx"
$okPpt = Try-ComSetDefault "PowerPoint.Show.8" ".ppt"

if (-not $okPptx -or -not $okPpt) {
    Write-Host ""
    Write-Host "COM restore unavailable. Opening Windows Default apps..." -ForegroundColor Yellow
    Start-Process "ms-settings:defaultapps?fileExtension=.pptx"
    Start-Sleep -Milliseconds 800
    Start-Process "ms-settings:defaultapps?fileExtension=.ppt"
    Write-Host "In Settings, pick WPS Presentation or Microsoft PowerPoint for each type."
    Write-Host "Or: right-click a .pptx -> Open with -> Choose another app -> pick app -> Always."
}

Write-Host ""
Write-Host "UserChoice .pptx:" -ForegroundColor Cyan
reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.pptx\UserChoice" /v ProgId 2>$null
Write-Host "UserChoice .ppt:" -ForegroundColor Cyan
reg query "HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.ppt\UserChoice" /v ProgId 2>$null
