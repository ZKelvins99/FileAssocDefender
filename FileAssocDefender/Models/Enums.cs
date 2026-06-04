namespace FileAssocDefender.Models;

public enum AssociationStatus
{
    Healthy,
    Hijacked,
    Unknown
}

public enum AssociationSource
{
    UserChoice,
    HkcuClasses,
    Hkcr,
    None
}

public enum LogEventType
{
    System,
    Scan,
    Hijack,
    Repair,
    Error
}

public enum FixStatus
{
    Success,
    Failed,
    Skipped
}
