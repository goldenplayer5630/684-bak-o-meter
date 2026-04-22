namespace _684BakOMeter.Web.Services;

/// <summary>
/// Detection thresholds for the manual-baseline chug flow.
/// All values are raw decimal readings from the scale sensor (not grams).
/// Defaults are tuned for the 684 Bak-O-Meter load cell setup.
/// </summary>
public class ChugSessionConfig
{
    /// <summary>
    /// Fraction of the captured baseline weight the smoothed reading must drop
    /// below before a lift is confirmed. E.g. 0.5 means the weight must fall
    /// below 50 % of the baseline. Applies symmetrically to return detection.
    /// </summary>
    public decimal LiftDropFactor { get; init; } = 0.5m;

    /// <summary>
    /// Number of consecutive readings above the return threshold
    /// required to confirm the glass has been returned and complete the chug.
    /// </summary>
    public int ReturnConfirmReadings { get; init; } = 3;
}
