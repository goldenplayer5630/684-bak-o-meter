using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Tracks the state of a single chug session on one scale.
/// Maintains a rolling average of recent measurements for noise filtering.
/// </summary>
public class ChugSession
{
    private readonly Queue<decimal> _recentValues = new();

    /// <summary>Number of values used for the rolling average.</summary>
    public const int AverageWindow = 2;

    public string SessionId { get; } = Guid.NewGuid().ToString("N")[..8];
    public int PlayerId { get; init; }
    public ChugType ChugType { get; init; }
    public int ScaleNumber { get; init; }
    public ChugSessionState State { get; set; } = ChugSessionState.WaitingForFull;

    public decimal CurrentAverage { get; private set; }
    public decimal? LiftWeight { get; set; }
    public DateTime? StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }

    /// <summary>Final duration in ms (only set when Completed).</summary>
    public int? DurationMs => StartTime.HasValue && EndTime.HasValue
        ? (int)(EndTime.Value - StartTime.Value).TotalMilliseconds
        : null;

    /// <summary>Live elapsed time in ms (ticks while Running).</summary>
    public int ElapsedMs => State == ChugSessionState.Running && StartTime.HasValue
        ? (int)(DateTime.UtcNow - StartTime.Value).TotalMilliseconds
        : DurationMs ?? 0;

    /// <summary>Whether enough values have been collected for a reliable average.</summary>
    public bool HasEnoughValues => _recentValues.Count >= AverageWindow;

    /// <summary>Adds a new measurement value to the rolling window and recalculates the average.</summary>
    public void AddValue(decimal value)
    {
        _recentValues.Enqueue(value);
        while (_recentValues.Count > AverageWindow)
            _recentValues.Dequeue();
        CurrentAverage = _recentValues.Average();
    }

    /// <summary>Marks the session as Running and records the start time.</summary>
    public void MarkStarted()
    {
        StartTime = DateTime.UtcNow;
        State = ChugSessionState.Running;
    }

    /// <summary>Marks the session as Completed and records the end time.</summary>
    public void MarkCompleted()
    {
        EndTime = DateTime.UtcNow;
        State = ChugSessionState.Completed;
    }

    /// <summary>Marks the session as Invalid (full glass placed back without drinking).</summary>
    public void MarkInvalid()
    {
        EndTime = DateTime.UtcNow;
        State = ChugSessionState.Invalid;
    }
}
