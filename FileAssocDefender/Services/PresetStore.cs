using System.IO;
using System.Text.Json;
using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class PresetStore
{
    private readonly List<Preset> _presets = [];

    public IReadOnlyList<Preset> Presets => _presets;

    public void Load()
    {
        _presets.Clear();
        _presets.AddRange(BuiltInPresets());

        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "presets.json");
        if (!File.Exists(path))
        {
            return;
        }

        var json = File.ReadAllText(path);
        var loaded = JsonSerializer.Deserialize<List<Preset>>(json, JsonOptions);
        if (loaded is { Count: > 0 })
        {
            _presets.Clear();
            _presets.AddRange(loaded);
        }
    }

    public HijackSignatures LoadHijackSignatures()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "hijack-signatures.json");
        if (!File.Exists(path))
        {
            return new HijackSignatures();
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<HijackSignatures>(json, JsonOptions) ?? new HijackSignatures();
    }

    private static List<Preset> BuiltInPresets() =>
    [
        new Preset { Extension = ".docx", ProgId = "Word.Document.12", DisplayName = "Word 文档", Category = "Word", ExpectedExePattern = "*WINWORD.EXE" },
        new Preset { Extension = ".doc", ProgId = "Word.Document.8", DisplayName = "Word 97-2003 文档", Category = "Word", ExpectedExePattern = "*WINWORD.EXE" },
        new Preset { Extension = ".xlsx", ProgId = "Excel.Sheet.12", DisplayName = "Excel 工作簿", Category = "Excel", ExpectedExePattern = "*EXCEL.EXE" },
        new Preset { Extension = ".xls", ProgId = "Excel.Sheet.8", DisplayName = "Excel 97-2003 工作簿", Category = "Excel", ExpectedExePattern = "*EXCEL.EXE" },
        new Preset { Extension = ".pptx", ProgId = "PowerPoint.Show.12", DisplayName = "PowerPoint 演示文稿", Category = "PowerPoint", ExpectedExePattern = "*POWERPNT.EXE" },
        new Preset { Extension = ".ppt", ProgId = "PowerPoint.Show.8", DisplayName = "PowerPoint 97-2003 演示文稿", Category = "PowerPoint", ExpectedExePattern = "*POWERPNT.EXE" }
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };
}
