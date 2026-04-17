using System.Collections.Concurrent;
using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Singleton service that manages chug sessions driven by scale measurements.
/// Maintains a per-scale state machine (WaitingForFull → Ready → Running → Completed)
/// and pushes live updates to the frontend via SignalR.
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

        // Add to rolling average
        session.AddValue(value);

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

            // Glass placed back — compare to the weight when it was lifted
            case ChugSessionState.Running when avg >= _config.EmptyThreshold:
            {
                var liftWeight = session.LiftWeight ?? _config.FullThreshold;
                var isStillFull = avg > liftWeight - 1000m;

                if (isStillFull)
                {
                    session.MarkInvalid();
                    _logger.LogInformation("Session {Id}: Glass placed back at ~same weight — invalid! (avg={Avg:F0}, liftWeight={Lift:F0})", session.SessionId, avg, liftWeight);
                    await _hubContext.Clients.All.SendAsync("ChugInvalid", new
                    {
                        sessionId = session.SessionId,
                        scaleNumber = session.ScaleNumber,
                        playerId = session.PlayerId,
                    }, ct);
                }
                else
                {
                    session.MarkCompleted();
                    _logger.LogInformation("Session {Id}: Lighter glass back — completed! Duration={Duration}ms (avg={Avg:F0}, liftWeight={Lift:F0})", session.SessionId, session.DurationMs, avg, liftWeight);
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
                break;
            }
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
