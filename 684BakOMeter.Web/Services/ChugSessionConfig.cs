namespace _684BakOMeter.Web.Services;

/// <summary>
/// Scale thresholds used by the chug detection state machine.
/// Values are raw decimal readings from the scale sensor (not grams).
/// 
/// No glass  : below <see cref="EmptyThreshold"/>
/// Empty glass: between <see cref="EmptyThreshold"/> and <see cref="FullThreshold"/>
/// Full glass : above <see cref="FullThreshold"/>
/// </summary>
public class ChugSessionConfig
{
    /// <summary>Above this value an empty glass is on the scale (≥ 50 000).</summary>
    public decimal EmptyThreshold { get; init; } = 50_000m;

    /// <summary>Above this value the glass is considered full (≥ 75 000).</summary>
    public decimal FullThreshold { get; init; } = 70_000m;

    /// <summary>
    /// Tolerance for the invalid check. If the glass comes back within this
    /// range of the weight when it was lifted, it's considered not drunk.
    /// </summary>
    public decimal InvalidTolerance { get; init; } = 10_000m;
}
