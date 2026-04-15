namespace _684BakOMeter.Web.Services;

/// <summary>
/// Handles incoming scale measurements received over the serial protocol
/// and will contain chug/timing business logic in the future.
/// </summary>
public class ChugService
{
    private readonly ILogger<ChugService> _logger;

    public ChugService(ILogger<ChugService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Processes a weight measurement from one of the connected scales.
    /// </summary>
    /// <param name="scaleNumber">Which scale produced the reading (1 or 2).</param>
    /// <param name="value">The measured weight value.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task HandleMeasurementAsync(int scaleNumber, decimal value, CancellationToken ct = default)
    {
        _logger.LogInformation("Scale {ScaleNumber} measurement received: {Value}g", scaleNumber, value);

        // TODO: implement chug detection / timing logic based on weight changes
        return Task.CompletedTask;
    }
}
