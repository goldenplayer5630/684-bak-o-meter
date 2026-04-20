using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Tracks the state of a single chug session on one scale.
/// Maintains a rolling average of recent measurements for noise filtering,
/// and a separate validation buffer that ignores initial impact spikes.
/// </summary>
public class ChugSession
{
    private readonly Queue<decimal> _recentValues = new();

    /// <summary>
    /// Validation buffer: collects values during the Validating phase,
    /// skipping the first few readings to ignore the return impact spike.
    /// </summary>
    private readonly List<decimal> _validationValues = new();

    /// <summary>Number of values used for the rolling average during normal operation.</summary>
    public const int AverageWindow = 4;

    /// <summary>
    /// Number of initial readings to discard when the glass is placed back.
    /// These readings contain the impact spike from the glass hitting the scale.
    /// </summary>
    public const int ImpactSpikeIgnoreCount = 3;

    public string SessionId { get; } = Guid.NewGuid().ToString("N")[..8];
    public int PlayerId { get; init; }
    public ChugType ChugType { get; init; }
    public int ScaleNumber { get; init; }
    public ChugSessionState State { get; set; } = ChugSessionState.WaitingForFull;

    public decimal CurrentAverage { get; private set; }
    public decimal? LiftWeight { get; set; }
    public DateTime? StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }

    /// <summary>Total validation readings collected (including ignored spike readings).</summary>
    public int ValidationReadingCount => _validationValues.Count + _validationSpikeCount;
    private int _validationSpikeCount;

    /// <summary>Final duration in ms (only set when Completed).</summary>
    public int? DurationMs => StartTime.HasValue && EndTime.HasValue
        ? (int)(EndTime.Value - StartTime.Value).TotalMilliseconds
        : null;

    /// <summary>Live elapsed time in ms (ticks while Running, frozen during Validating/Completed).</summary>
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

    /// <summary>
    /// Adds a value to the validation buffer. The first <see cref="ImpactSpikeIgnoreCount"/>
    /// values are discarded to filter out the return impact spike.
    /// </summary>
    public void AddValidationValue(decimal value)
    {
        if (_validationSpikeCount < ImpactSpikeIgnoreCount)
        {
            _validationSpikeCount++;
            return;
        }
        _validationValues.Add(value);
    }

    /// <summary>
    /// Returns the average of the validation buffer (post-spike values only),
    /// or null if no settled values have been collected yet.
    /// </summary>
    public decimal? SettledValidationAverage =>
        _validationValues.Count > 0 ? _validationValues.Average() : null;

    /// <summary>Marks the session as Running and records the start time.</summary>
    public void MarkStarted()
    {
        StartTime = DateTime.UtcNow;
        State = ChugSessionState.Running;
    }

    /// <summary>Freezes the end time for duration calculation without changing state.</summary>
    public void FreezeEndTime()
    {
        EndTime = DateTime.UtcNow;
    }

    /// <summary>Marks the session as Invalid (full glass placed back without drinking).</summary>
    public void MarkInvalid()
    {
        EndTime = DateTime.UtcNow;
        State = ChugSessionState.Invalid;
    }
}
