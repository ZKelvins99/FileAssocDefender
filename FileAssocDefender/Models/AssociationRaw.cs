namespace FileAssocDefender.Models;

public sealed class AssociationRaw
{
    public required string Extension { get; init; }
    public string ProgId { get; init; } = string.Empty;
    public string CommandLine { get; init; } = string.Empty;
    public string ExePath { get; init; } = string.Empty;
    public AssociationSource Source { get; init; } = AssociationSource.None;
}
