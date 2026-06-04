namespace FileAssocDefender.Models;

public sealed class OfficeInstallation
{
    public bool IsInstalled { get; init; }
    public string WordExePath { get; init; } = string.Empty;
    public string ExcelExePath { get; init; } = string.Empty;
    public string PowerPointExePath { get; init; } = string.Empty;
    public string WordProgId { get; init; } = "Word.Document.12";
    public string ExcelProgId { get; init; } = "Excel.Sheet.12";
    public string PowerPointProgId { get; init; } = "PowerPoint.Show.12";
}
