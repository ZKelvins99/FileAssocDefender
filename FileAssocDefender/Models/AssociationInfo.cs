using System.Windows.Media;

namespace FileAssocDefender.Models;

public sealed class AssociationInfo
{
    public required string Extension { get; init; }
    public required string FileTypeLabel { get; init; }
    public required string Category { get; init; }

    public string CurrentProgId { get; init; } = string.Empty;
    public string CurrentAppName { get; init; } = string.Empty;
    public string CurrentExePath { get; init; } = string.Empty;
    public ImageSource? CurrentIcon { get; init; }

    public string TargetProgId { get; init; } = string.Empty;
    public string TargetAppName { get; init; } = string.Empty;
    public string TargetExePath { get; init; } = string.Empty;
    public ImageSource? TargetIcon { get; init; }

    public AssociationStatus Status { get; init; }
    public AssociationSource Source { get; init; }
    public string StatusMessage { get; init; } = string.Empty;
    public string CommandLine { get; init; } = string.Empty;
}
