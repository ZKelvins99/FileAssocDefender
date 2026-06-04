# FileAssocDefender

文件打开方式守护工具 — 展示 Office 等文件类型的当前默认程序（含图标），识别 WPS 等劫持，并修复为 Microsoft Office 默认打开。

## 技术栈

- .NET 10 + WPF + WPF-UI（Win11 Mica 风格）
- WiX Toolset v5（MSI 安装包）
- self-contained 单文件 EXE 发布

## 项目结构

```
FileAssocDefender/
├── FileAssocDefender.slnx          # 解决方案
├── global.json                     # SDK 10.0.300
├── Directory.Build.props           # 统一版本与规范
├── FileAssocDefender/              # WPF 主程序
├── FileAssocDefender.Tests/        # 单元测试
├── Installer/                      # WiX MSI 工程
├── scripts/build.ps1               # 一键构建 / 发布 / 打包
└── artifacts/                      # 构建产物（git 忽略）
    ├── publish/win-x64/            # 单文件 EXE
    └── package/                    # MSI 安装包
```

## 快速开始

### 开发构建

```powershell
dotnet build FileAssocDefender/FileAssocDefender.csproj
dotnet run --project FileAssocDefender/FileAssocDefender.csproj
```

### 发布 EXE + MSI

```powershell
# 全部：编译 → 测试 → 发布 EXE → 打包 MSI
.\scripts\build.ps1 -Target All -Configuration Release

# 仅发布单文件 EXE
.\scripts\build.ps1 -Target Publish -Configuration Release

# 仅打包 MSI（需先 Publish）
.\scripts\build.ps1 -Target Package -Configuration Release
```

产物路径：

| 格式 | 路径 |
|------|------|
| EXE | `artifacts/publish/win-x64/FileAssocDefender.exe` |
| MSI | `artifacts/package/FileAssocDefender.msi` |

## 权限说明

应用需要**管理员权限**运行（`app.manifest` 已配置 UAC 提权），以便修改文件关联注册表。

## 开发计划

详见 [建设方案.md](建设方案.md)。
