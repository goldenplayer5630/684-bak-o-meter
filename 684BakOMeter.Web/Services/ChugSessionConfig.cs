namespace _684BakOMeter.Web.Services;

/// <summary>
/// Detection thresholds for the manual-baseline chug flow.
/// All values are raw decimal readings from the scale sensor (not grams).
/// Defaults are tuned for the 684 Bak-O-Meter load cell setup.
/// </summary>
public class ChugSessionConfig
{
    /// <summary>
    /// Max allowed spread (max − min) across the rolling window for the
    /// baseline to be considered stable enough to capture.
    /// </summary>
    public decimal BaselineMaxDeviation { get; init; } = 800m;

    /// <summary>
    /// Minimum raw weight required when confirming the baseline.
    /// Prevents accepting a near-empty scale as a valid baseline.
    /// </summary>
    public decimal BaselineMinWeight { get; init; } = 40_000m;

    /// <summary>
    /// How far below the captured baseline the smoothed weight must drop
    /// before a glass lift is confirmed.
    /// </summary>
    public decimal LiftDropThreshold { get; init; } = 10_000m;

    /// <summary>
    /// Smoothed weight must exceed this value for each reading that counts
    /// toward return confirmation. Must be well above the bare-scale reading.
    /// </summary>
    public decimal ReturnConfirmMinWeight { get; init; } = 40_000m;

    /// <summary>
    /// Number of consecutive readings above <see cref="ReturnConfirmMinWeight"/>
    /// required to confirm the glass has been returned and complete the chug.
    /// </summary>
    public int ReturnConfirmReadings { get; init; } = 3;
}
