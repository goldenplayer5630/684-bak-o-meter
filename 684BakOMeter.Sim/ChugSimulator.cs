namespace _685BakOMeter.Sim;

/// <summary>
/// Simulates a realistic chug sequence including glass removal, hard impact, and settling.
/// </summary>
public class ChugSimulator
{
    private readonly Random _random = new();
    private readonly Action<int> _sendWeightCallback;
    private readonly Action<string> _logCallback;
    private CancellationTokenSource? _currentSimulation;

    public ChugSimulator(Action<int> sendWeightCallback, Action<string> logCallback)
    {
        _sendWeightCallback = sendWeightCallback ?? throw new ArgumentNullException(nameof(sendWeightCallback));
        _logCallback = logCallback ?? throw new ArgumentNullException(nameof(logCallback));
    }

    /// <summary>
    /// Indicates whether a chug simulation is currently running.
    /// </summary>
    public bool IsRunning => _currentSimulation != null && !_currentSimulation.IsCancellationRequested;

    /// <summary>
    /// Stops any currently running chug simulation.
    /// </summary>
    public void Stop()
    {
        _currentSimulation?.Cancel();
        _currentSimulation = null;
    }

    /// <summary>
    /// Simulates a glass chug sequence.
    /// </summary>
    public async Task SimulateGlassChugAsync()
    {
        await SimulateChugAsync(
            startValue: SimulatorConfig.FullGlass,
            endValue: SimulatorConfig.EmptyGlass,
            startMargin: SimulatorConfig.FullGlassMargin,
            endMargin: SimulatorConfig.EmptyGlassMargin,
            "Glass");
    }

    /// <summary>
    /// Simulates a pul chug sequence.
    /// </summary>
    public async Task SimulatePulChugAsync()
    {
        await SimulateChugAsync(
            startValue: SimulatorConfig.FullPul,
            endValue: SimulatorConfig.EmptyPul,
            startMargin: SimulatorConfig.FullPulMargin,
            endMargin: SimulatorConfig.EmptyPulMargin,
            "Pul");
    }

    /// <summary>
    /// Core chug simulation logic.
    /// Sequence: stable start → removed → hard impact → settle to end value
    /// </summary>
    private async Task SimulateChugAsync(
        int startValue,
        int endValue,
        int startMargin,
        int endMargin,
        string label)
    {
        if (IsRunning)
        {
            _logCallback($"⚠️ Simulation already running. Stop it first.");
            return;
        }

        _currentSimulation = new CancellationTokenSource();
        var token = _currentSimulation.Token;

        try
        {
            _logCallback($"🎬 Starting {label} chug simulation...");

            // Phase 1: Stable starting value (full glass/pul)
            _logCallback($"   Phase 1: Stable at {label} full value");
            for (int i = 0; i < 5; i++)
            {
                if (token.IsCancellationRequested) return;
                SendRandomizedWeight(startValue, startMargin);
                await Task.Delay(SimulatorConfig.SimulationUpdateIntervalMs, token);
            }

            // Phase 2: Removed (nothing on scale)
            var removalDuration = _random.Next(
                SimulatorConfig.RemovalMinMs,
                SimulatorConfig.RemovalMaxMs);
            _logCallback($"   Phase 2: Removed for {removalDuration}ms");

            var removalSteps = removalDuration / SimulatorConfig.SimulationUpdateIntervalMs;
            for (int i = 0; i < removalSteps; i++)
            {
                if (token.IsCancellationRequested) return;
                SendRandomizedWeight(SimulatorConfig.Nothing, SimulatorConfig.NothingMargin);
                await Task.Delay(SimulatorConfig.SimulationUpdateIntervalMs, token);
            }

            // Phase 3: Hard impact spike (overshoot)
            _logCallback($"   Phase 3: Hard impact spike for {SimulatorConfig.ImpactDurationMs}ms");
            var impactSteps = SimulatorConfig.ImpactDurationMs / SimulatorConfig.SimulationUpdateIntervalMs;
            var impactValue = (int)(endValue * SimulatorConfig.ImpactOvershotMultiplier);

            for (int i = 0; i < impactSteps; i++)
            {
                if (token.IsCancellationRequested) return;
                // Gradually reduce spike from peak to normal
                var progress = (double)i / impactSteps;
                var currentValue = (int)(impactValue - (impactValue - endValue) * progress);
                SendRandomizedWeight(currentValue, endMargin);
                await Task.Delay(SimulatorConfig.SimulationUpdateIntervalMs, token);
            }

            // Phase 4: Settle to stable end value (empty glass/pul)
            _logCallback($"   Phase 4: Settled at {label} empty value");
            for (int i = 0; i < 10; i++)
            {
                if (token.IsCancellationRequested) return;
                SendRandomizedWeight(endValue, endMargin);
                await Task.Delay(SimulatorConfig.SimulationUpdateIntervalMs, token);
            }

            _logCallback($"✅ {label} chug simulation complete!");
        }
        catch (OperationCanceledException)
        {
            _logCallback($"⏹️ {label} chug simulation stopped.");
        }
        catch (Exception ex)
        {
            _logCallback($"❌ Error during simulation: {ex.Message}");
        }
        finally
        {
            _currentSimulation = null;
        }
    }

    /// <summary>
    /// Sends a randomized weight value around the base value.
    /// </summary>
    private void SendRandomizedWeight(int baseValue, int margin)
    {
        var randomOffset = _random.Next(-margin, margin + 1);
        var value = baseValue + randomOffset;
        _sendWeightCallback(value);
    }
}
