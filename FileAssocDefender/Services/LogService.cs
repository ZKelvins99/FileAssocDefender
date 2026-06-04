using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class LogService
{
    private readonly List<LogEntry> _entries = [];
    private readonly object _sync = new();

    public event Action? EntriesChanged;

    public IReadOnlyList<LogEntry> Entries
    {
        get
        {
            lock (_sync)
            {
                return _entries.ToList();
            }
        }
    }

    public void Info(string message) => Add(LogEventType.System, message);
    public void Warn(string message) => Add(LogEventType.Hijack, message);
    public void Error(string message) => Add(LogEventType.Error, message);

    public void AssociationChanged(string extension, string from, string to)
        => Add(LogEventType.Repair, $"{extension}: {from} → {to}");

    public void Clear()
    {
        lock (_sync)
        {
            _entries.Clear();
        }

        EntriesChanged?.Invoke();
    }

    private void Add(LogEventType type, string message)
    {
        lock (_sync)
        {
            _entries.Insert(0, new LogEntry
            {
                EventType = type,
                Message = message,
                Timestamp = DateTimeOffset.Now
            });
        }

        EntriesChanged?.Invoke();
    }
}
