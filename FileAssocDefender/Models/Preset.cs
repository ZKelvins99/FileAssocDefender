namespace FileAssocDefender.Models;

public sealed class Preset
{
    public required string Extension { get; init; }
    public required string ProgId { get; init; }
    public required string DisplayName { get; init; }
    public required string Category { get; init; }
    public required string ExpectedExePattern { get; init; }
}
