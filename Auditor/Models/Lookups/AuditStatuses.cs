namespace Auditor.Models.Lookups;

/// <summary>Lifecycle of an audit schedule.</summary>
public static class ScheduleStatuses
{
    public const string Scheduled = "SCHEDULED";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";
    public const string Cancelled = "CANCELLED";

    public static string Label(string? status) => status switch
    {
        Scheduled => "Scheduled",
        InProgress => "In progress",
        Completed => "Completed",
        Cancelled => "Cancelled",
        _ => status ?? "—",
    };

    public static string PillClass(string? status) => status switch
    {
        Scheduled => "status-pill-info",
        InProgress => "status-pill-warning",
        Completed => "status-pill-active",
        _ => "status-pill-inactive",
    };
}

/// <summary>Lifecycle of an audit execution (one auditor's run of a schedule).</summary>
public static class ExecutionStatuses
{
    public const string PendingAcceptance = "PENDING_ACCEPTANCE";
    public const string InProgress = "IN_PROGRESS";
    public const string Completed = "COMPLETED";
    public const string Closed = "CLOSED";
    public const string Rejected = "REJECTED";
    public const string Declined = "DECLINED";

    public static string Label(string? status) => status switch
    {
        PendingAcceptance => "Awaiting acceptance",
        InProgress => "In progress",
        Completed => "Submitted for review",
        Closed => "Closed",
        Rejected => "Returned for changes",
        Declined => "Declined",
        _ => status ?? "—",
    };

    public static string PillClass(string? status) => status switch
    {
        PendingAcceptance => "status-pill-info",
        InProgress => "status-pill-warning",
        Completed => "status-pill-active",
        Closed => "status-pill-active",
        Rejected => "status-pill-danger",
        _ => "status-pill-inactive",
    };
}
