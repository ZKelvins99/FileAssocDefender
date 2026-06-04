using FileAssocDefender.Models;

namespace FileAssocDefender.Services;

public sealed class Guardian : IDisposable
{
    private readonly AssociationScanner _scanner;
    private readonly AssociationFixer _fixer;
    private readonly LogService _logService;
    private System.Threading.Timer? _timer;
    private bool _autoRepair;

    public event Action<IReadOnlyList<AssociationInfo>>? ScanCompleted;
    public event Action<AssociationInfo>? HijackDetected;
    public event Action<AssociationInfo>? Repaired;

    public int PollingIntervalMs { get; set; } = 10_000;

    public bool AutoRepair
    {
        get => _autoRepair;
        set => _autoRepair = value;
    }

    public Guardian(
        AssociationScanner scanner,
        AssociationFixer fixer,
        LogService logService)
    {
        _scanner = scanner;
        _fixer = fixer;
        _logService = logService;
    }

    public void Start()
    {
        Stop();
        _timer = new Timer(_ => RunScan(), null, TimeSpan.Zero, TimeSpan.FromMilliseconds(PollingIntervalMs));
        _logService.Info("守护引擎已启动");
    }

    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    public IReadOnlyList<AssociationInfo> ScanNow()
    {
        var results = _scanner.Scan();
        ScanCompleted?.Invoke(results);
        return results;
    }

    private void RunScan()
    {
        var results = ScanNow();
        foreach (var item in results.Where(r => r.Status == AssociationStatus.Hijacked))
        {
            HijackDetected?.Invoke(item);
            _logService.Warn($"{item.Extension} 被 {item.CurrentAppName} 劫持");

            if (_autoRepair)
            {
                var fix = _fixer.Fix(item);
                if (fix.Status == FixStatus.Success)
                {
                    Repaired?.Invoke(item);
                }
            }
        }
    }

    public void Dispose() => Stop();
}
