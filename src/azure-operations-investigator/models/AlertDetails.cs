public sealed record AlertDetails(
    string AlertId,
    string Severity,
    string Resource,
    string Region,
    string Signal,
    IReadOnlyList<string> NextSteps,
    AlertSafety Safety);

public sealed record AlertSafety(
    string AccessLevel,
    bool DestructiveAction);