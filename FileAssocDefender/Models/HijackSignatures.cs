namespace FileAssocDefender.Models;

public sealed class HijackSignatures
{
    public List<string> ProgIds { get; init; } = [];
    public List<string> ExePatterns { get; init; } = [];
}
