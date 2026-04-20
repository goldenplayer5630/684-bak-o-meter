using System.Collections.Concurrent;
using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Singleton service that manages chug sessions driven by scale measurements.
/// Maintains a per-scale state machine (WaitingForFull → Ready → Running → Completed)
/// and pushes live updates to the frontend via SignalR.
///
/// Persistence of completed attempts (including IsHighScore calculation) is handled
/// by <see cref="Data.Repositories.IChugAttemptRepository.AddAsync"/> — this service
/// only manages in-memory session state and real-time events.
/// </summary>
public class ChugService
{
    private readonly IHubContext<ChugHub> _hubContext;
    private readonly ILogger<ChugService> _logger;
    private readonly CalibrationService _calibrationService;
    private ChugSessionConfig _config = new();

    // One active session per scale (scale 1, scale 2)
    private readonly ConcurrentDictionary<int, ChugSession> _sessions = new();

    public ChugService(IHubContext<ChugHub> hubContext, ILogger<ChugService> logger, CalibrationService calibrationService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _calibrationService = calibrationService;
    }

    /// <summary>Creates a new chug session on the given scale, replacing any existing one.</summary>
    public ChugSession StartSession(int playerId, ChugType chugType, int scaleNumber)
    {
        // Build thresholds from calibration (glass vs pul)
        _config = _calibrationService.BuildSessionConfig(chugType);

        var session = new ChugSession
        {
            PlayerId = playerId,
            ChugType = chugType,
            ScaleNumber = scaleNumber,
        };

        _sessions[scaleNumber] = session;

        _logger.LogInformation(
            "Chug session {SessionId} started: player={PlayerId}, type={ChugType}, scale={Scale}",
            session.SessionId, playerId, chugType, scaleNumber);

        return session;
    }

    /// <summary>Returns the active session for a given scale, or null.</summary>
    public ChugSession? GetSession(int scaleNumber)
    {
        _sessions.TryGetValue(scaleNumber, out var session);
        return session;
    }

    /// <summary>Cancels and removes the active session on a given scale.</summary>
    public void CancelSession(int scaleNumber)
    {
        if (_sessions.TryRemove(scaleNumber, out var session))
            _logger.LogInformation("Chug session {SessionId} cancelled.", session.SessionId);
    }

    /// <summary>
    /// Processes a weight measurement from the serial protocol.
    /// Only acts on the measurement if there is an active session on the matching scale.
    /// </summary>
    public async Task HandleMeasurementAsync(int scaleNumber, decimal value, CancellationToken ct = default)
    {
        // Always broadcast the raw value so the calibration wizard can read it
        await _hubContext.Clients.All.SendAsync("ScaleRaw", new
        {
            scaleNumber,
            value = Math.Round(value, 1),
        }, ct);

        if (!_sessions.TryGetValue(scaleNumber, out var session))
            return;

        if (session.State is ChugSessionState.Completed or ChugSessionState.Invalid)
            return;

        // Add to rolling average (used for state-machine transitions)
        session.AddValue(value);

        // During validation, also collect readings in the spike-filtered buffer
        if (session.State == ChugSessionState.Validating)
            session.AddValidationValue(value);

        if (!session.HasEnoughValues)
        {
            await SendUpdateAsync(session, ct);
            return;
        }

        var avg = session.CurrentAverage;

        // --- State machine ---
        switch (session.State)
        {
            // Wait for a full glass on the scale
            case ChugSessionState.WaitingForFull when avg >= _config.FullThreshold:
                session.LiftWeight = avg;
                session.State = ChugSessionState.Ready;
                _logger.LogInformation("Session {Id}: Full glass detected (avg={Avg:F0})", session.SessionId, avg);
                break;

            // Glass still sitting on the scale — keep tracking the stable weight
            case ChugSessionState.Ready when avg >= _config.EmptyThreshold:
                session.LiftWeight = avg;
                break;

            // Glass lifted off the scale → start timer
            case ChugSessionState.Ready when avg < _config.EmptyThreshold:
                session.MarkStarted();
                _logger.LogInformation("Session {Id}: Glass lifted — timer started! (avg={Avg:F0})", session.SessionId, avg);
                await _hubContext.Clients.All.SendAsync("ChugStarted", new
                {
                    sessionId = session.SessionId,
                    scaleNumber = session.ScaleNumber,
                    playerId = session.PlayerId,
                    startTime = session.StartTime,
                }, ct);
                break;

            // Glass placed back — enter validating state (5-second settle period)
            case ChugSessionState.Running when avg >= _config.EmptyThreshold:
            {
                session.FreezeEndTime(); // freeze the timer now
                session.State = ChugSessionState.Validating;
                session.FreezeEndTime(); // freeze the timer now, state stays Validating
                _logger.LogInformation(
                    "Session {Id}: Glass placed back — entering {Seconds}s validation (avg={Avg:F0})",
                    session.SessionId, ChugSessionConfig.ValidationDelaySeconds, avg);
                _ = ValidateAfterDelayAsync(session, ct);
                break;
            }

            // While validating, keep collecting readings (handled above) but don't transition
            case ChugSessionState.Validating:
                break;
        }

        await SendUpdateAsync(session, ct);
    }

    /// <summary>
    /// Waits for the validation settle period, then determines whether the
    /// player actually drank. Uses a spike-filtered validation buffer so
    /// the initial impact of placing the glass back does not pollute the result.
    /// 
    /// Invalid = the settled weight is still close to the full-glass weight,
    /// meaning the player put a full glass back without drinking.
    /// 
    /// The check uses a relative threshold: if more than 70% of the liquid
    /// weight is still present, the chug is invalid. This adapts automatically
    /// to different calibration values and container types.
    /// </summary>
    private async Task ValidateAfterDelayAsync(ChugSession session, CancellationToken ct)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(ChugSessionConfig.ValidationDelaySeconds), ct);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        // Use the spike-filtered validation buffer if available, otherwise fall back to rolling average
        var settledAvg = session.SettledValidationAverage ?? session.CurrentAverage;
        var liftWeight = session.LiftWeight ?? _config.FullContainerWeight;

        // The weight of the liquid alone = full container − empty container.
        // We use calibration values rather than the live lift weight to avoid
        // per-session noise affecting the threshold.
        var liquidWeight = _config.FullContainerWeight - _config.EmptyContainerWeight;

        // Invalid if more than 70% of the liquid is still present.
        // This gives a very generous margin: even if 30% spills or the scale
        // drifts, the chug is still accepted. Only a clearly undrunk glass fails.
        const decimal InvalidFraction = 0.70m;
        var invalidThreshold = _config.EmptyContainerWeight + (liquidWeight * InvalidFraction);

        var isStillFull = settledAvg > invalidThreshold;

        _logger.LogInformation(
            "Session {Id}: Validation result — settledAvg={Settled:F0}, " +
            "emptyContainer={Empty:F0}, fullContainer={Full:F0}, " +
            "liquidWeight={Liquid:F0}, invalidThreshold={Threshold:F0}, " +
            "liftWeight={Lift:F0}, spikeFiltered={SpikeFiltered}, " +
            "validationReadings={Readings}, verdict={Verdict}",
            session.SessionId, settledAvg,
            _config.EmptyContainerWeight, _config.FullContainerWeight,
            liquidWeight, invalidThreshold,
            liftWeight, session.SettledValidationAverage.HasValue,
            session.ValidationReadingCount,
            isStillFull ? "INVALID" : "COMPLETED");

        if (isStillFull)
        {
            session.MarkInvalid();
            _logger.LogWarning(
                "Session {Id}: INVALID — glass still full. " +
                "Settled avg {Settled:F0} > threshold {Threshold:F0} " +
                "(>{Pct}% of liquid still present)",
                session.SessionId, settledAvg, invalidThreshold, InvalidFraction * 100);

            await _hubContext.Clients.All.SendAsync("ChugInvalid", new
            {
                sessionId = session.SessionId,
                scaleNumber = session.ScaleNumber,
                playerId = session.PlayerId,
            }, ct);
        }
        else
        {
            session.State = ChugSessionState.Completed;
            _logger.LogInformation(
                "Session {Id}: COMPLETED — Duration={Duration}ms. " +
                "Settled avg {Settled:F0} <= threshold {Threshold:F0}",
                session.SessionId, session.DurationMs, settledAvg, invalidThreshold);

            await _hubContext.Clients.All.SendAsync("ChugCompleted", new
            {
                sessionId = session.SessionId,
                scaleNumber = session.ScaleNumber,
                playerId = session.PlayerId,
                durationMs = session.DurationMs,
                startTime = session.StartTime,
                endTime = session.EndTime,
            }, ct);
        }

        await SendUpdateAsync(session, ct);
    }

    /// <summary>Pushes a state/weight snapshot to all connected SignalR clients.</summary>
    private async Task SendUpdateAsync(ChugSession session, CancellationToken ct)
    {
        await _hubContext.Clients.All.SendAsync("ChugUpdate", new
        {
            sessionId = session.SessionId,
            scaleNumber = session.ScaleNumber,
            playerId = session.PlayerId,
            state = session.State.ToString(),
            currentAverage = Math.Round(session.CurrentAverage, 1),
            elapsedMs = session.ElapsedMs,
            isReady = session.State == ChugSessionState.Ready,
            isRunning = session.State == ChugSessionState.Running,
            isCompleted = session.State == ChugSessionState.Completed,
            durationMs = session.DurationMs,
        }, ct);
    }
}
