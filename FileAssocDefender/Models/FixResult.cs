namespace FileAssocDefender.Models;

public sealed class FixResult
{
    public required string Extension { get; init; }
    public FixStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
}
