using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Tracks the state of a single chug session on one scale.
///
/// Flow: WaitingForBaseline → ReadyToLift → Running → Completed
///
/// The player manually confirms the baseline weight by pressing SPACE while
/// the glass is still on the scale. The timer starts when the glass is lifted,
/// and the session completes when the glass is returned and stable.
/// </summary>
public class ChugSession
{
    private readonly Queue<decimal> _recentValues = new();
    private int _returnConfirmCount;

    /// <summary>Number of values kept in the rolling average window.</summary>
    public const int AverageWindow = 4;

    public string SessionId { get; } = Guid.NewGuid().ToString("N")[..8];
    public int PlayerId { get; init; }
    public ChugType ChugType { get; init; }
    public int ScaleNumber { get; init; }
    public ChugSessionState State { get; set; } = ChugSessionState.WaitingForBaseline;

    /// <summary>Current smoothed weight (rolling average of last <see cref="AverageWindow"/> readings).</summary>
    public decimal CurrentAverage { get; private set; }

    /// <summary>Smoothed weight from the previous reading cycle, used for delta calculations.</summary>
    public decimal PreviousAverage { get; private set; }

    /// <summary>Baseline weight captured when the player confirmed with SPACE.</summary>
    public decimal? BaselineWeight { get; private set; }

    public DateTime? StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }

    /// <summary>Final duration in ms — set once the session reaches Completed.</summary>
    public int? DurationMs => StartTime.HasValue && EndTime.HasValue
        ? (int)(EndTime.Value - StartTime.Value).TotalMilliseconds
        : null;

    /// <summary>Live elapsed time in ms. Ticks while Running; frozen once Completed.</summary>
    public int ElapsedMs => State == ChugSessionState.Running && StartTime.HasValue
        ? (int)(DateTime.UtcNow - StartTime.Value).TotalMilliseconds
        : DurationMs ?? 0;

    /// <summary>True once the rolling window is fully populated and the average is reliable.</summary>
    public bool HasEnoughValues => _recentValues.Count >= AverageWindow;

    /// <summary>Adds a new measurement to the rolling window and recalculates the average.</summary>
    public void AddValue(decimal value)
    {
        PreviousAverage = CurrentAverage;
        _recentValues.Enqueue(value);
        while (_recentValues.Count > AverageWindow)
            _recentValues.Dequeue();
        CurrentAverage = _recentValues.Average();
    }

    /// <summary>
    /// Returns true when the rolling window is full, the average exceeds
    /// <paramref name="minWeight"/>, and all values in the window are within
    /// <paramref name="maxDeviation"/> of each other.
    /// </summary>
    public bool IsStableForBaseline(decimal maxDeviation, decimal minWeight)
    {
        if (!HasEnoughValues) return false;
        if (CurrentAverage < minWeight) return false;
        var values = _recentValues.ToList();
        return values.Max() - values.Min() <= maxDeviation;
    }

    /// <summary>
    /// Captures the current average as the session baseline and transitions to ReadyToLift.
    /// Call only after <see cref="IsStableForBaseline"/> returns true.
    /// </summary>
    public void CaptureBaseline()
    {
        BaselineWeight = CurrentAverage;
        _returnConfirmCount = 0;
        State = ChugSessionState.ReadyToLift;
    }

    /// <summary>
    /// Returns true when the smoothed weight has dropped far enough below the
    /// baseline to confirm the glass was lifted.
    /// </summary>
    public bool IsLifted(decimal liftDropThreshold)
        => BaselineWeight.HasValue && CurrentAverage < BaselineWeight.Value - liftDropThreshold;

    /// <summary>Starts the timer and transitions to Running.</summary>
    public void MarkStarted()
    {
        StartTime = DateTime.UtcNow;
        State = ChugSessionState.Running;
        _returnConfirmCount = 0;
    }

    /// <summary>
    /// Accumulates return-confirmation readings and returns true once the glass
    /// is confirmed back on the scale. Must be called on every measurement while Running.
    ///
    /// Strategy: count consecutive readings at or above <paramref name="confirmMinWeight"/>.
    /// The weight was near zero while lifted, so a sustained rise above the minimum
    /// naturally captures the return kick and its follow-through in a single mechanism.
    /// </summary>
    public bool TrackReturn(decimal confirmMinWeight, int confirmReadings)
    {
        if (CurrentAverage >= confirmMinWeight)
            _returnConfirmCount++;
        else
            _returnConfirmCount = 0;

        return _returnConfirmCount >= confirmReadings;
    }

    /// <summary>Stops the timer and transitions to Completed.</summary>
    public void MarkCompleted()
    {
        EndTime = DateTime.UtcNow;
        State = ChugSessionState.Completed;
    }
}
