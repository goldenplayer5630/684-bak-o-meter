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
    /// <summary>Above this value an empty glass is on the scale.</summary>
    public decimal EmptyThreshold { get; init; } = 50_000m;

    /// <summary>Above this value the glass is considered full.</summary>
    public decimal FullThreshold { get; init; } = 70_000m;

    /// <summary>
    /// The calibrated weight of the empty container (glass or pul).
    /// Used to determine if a returned glass was drunk: the settled weight
    /// after return must be below this value plus a tolerance margin.
    /// </summary>
    public decimal EmptyContainerWeight { get; init; } = 67_000m;

    /// <summary>
    /// The calibrated weight of the full container.
    /// Used together with <see cref="EmptyContainerWeight"/> to compute
    /// relative tolerance margins for the invalid check.
    /// </summary>
    public decimal FullContainerWeight { get; init; } = 82_000m;

    /// <summary>Seconds to wait during validation for the sensor to settle.</summary>
    public const int ValidationDelaySeconds = 5;
}
