namespace FileAssocDefender.Models;

public sealed class LogEntry
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public LogEventType EventType { get; init; }
    public string Message { get; init; } = string.Empty;
}
