namespace FileAssocDefender.Models;

public sealed class AssociationBackup
{
    public required string Extension { get; init; }
    public string ProgId { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
    public AssociationSource Source { get; init; } = AssociationSource.None;
    public DateTimeOffset BackedUpAt { get; init; } = DateTimeOffset.Now;
}
