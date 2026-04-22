using System.Collections.Concurrent;
using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Singleton service that manages chug sessions on the manual-baseline flow.
///
/// Flow per scale:
///   1. <see cref="StartSession"/> — creates a session in WaitingForBaseline.
///   2. <see cref="ConfirmBaseline"/> — called when the player presses SPACE with a
///      full glass on the scale; captures the stable weight as the lift reference.
///   3. <see cref="HandleMeasurementAsync"/> — drives the state machine:
///        ReadyToLift  ? detects glass lift ? starts timer ? Running
///        Running      ? detects glass return (sustained weight above threshold) ? Completed
///
/// Persistence is handled by the repository; this service only manages in-memory
/// session state and SignalR real-time events.
/// </summary>
public class ChugService(IHubContext<ChugHub> hubContext, ILogger<ChugService> logger)
{
    private readonly ChugSessionConfig _config = new();
    private readonly ConcurrentDictionary<int, ChugSession> _sessions = new();

    /// <summary>Creates a new chug session on the given scale, replacing any existing one.</summary>
    public ChugSession StartSession(int playerId, ChugType chugType, int scaleNumber)
    {
        var session = new ChugSession
        {
            PlayerId = playerId,
            ChugType = chugType,
            ScaleNumber = scaleNumber,
        };

        _sessions[scaleNumber] = session;

        logger.LogInformation(
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
            logger.LogInformation("Chug session {SessionId} cancelled.", session.SessionId);
    }

    /// <summary>
    /// Attempts to capture the current stable weight as the session baseline.
    /// Returns a failure result if there is no active session, if the session
    /// is not in WaitingForBaseline, or if the weight is not stable enough.
    /// </summary>
    public ConfirmBaselineResult ConfirmBaseline(int scaleNumber)
    {
        if (!_sessions.TryGetValue(scaleNumber, out var session))
            return ConfirmBaselineResult.Fail("Geen actieve sessie op deze weegschaal.");

        if (session.State != ChugSessionState.WaitingForBaseline)
            return ConfirmBaselineResult.Fail($"Sessie is al in staat {session.State}.");

        if (!session.HasEnoughValues)
            return ConfirmBaselineResult.Fail("Nog niet genoeg metingen. Houd het glas stil.");

        if (!session.IsStableForBaseline(_config.BaselineMaxDeviation, _config.BaselineMinWeight))
            return ConfirmBaselineResult.Fail("Gewicht is niet stabiel. Houd het glas stil.");

        session.CaptureBaseline();

        logger.LogInformation(
            "Session {Id}: Baseline captured — weight={Weight:F0}",
            session.SessionId, session.BaselineWeight);

        return ConfirmBaselineResult.Ok(session.BaselineWeight!.Value, session.State.ToString());
    }

    /// <summary>
    /// Processes a weight measurement from the serial protocol.
    /// Always broadcasts the raw value, then drives the state machine
    /// for any active session on the given scale.
    /// </summary>
    public async Task HandleMeasurementAsync(int scaleNumber, decimal value, CancellationToken ct = default)
    {
        await hubContext.Clients.All.SendAsync("ScaleRaw", new
        {
            scaleNumber,
            value = Math.Round(value, 1),
        }, ct);

        if (!_sessions.TryGetValue(scaleNumber, out var session))
            return;

        if (session.State == ChugSessionState.Completed)
            return;

        session.AddValue(value);

        if (!session.HasEnoughValues)
        {
            await SendUpdateAsync(session, ct);
            return;
        }

        switch (session.State)
        {
            // Waiting for player to confirm baseline — no automatic transitions.
            case ChugSessionState.WaitingForBaseline:
                break;

            // Baseline confirmed — detect glass lift.
            case ChugSessionState.ReadyToLift when session.IsLifted(_config.LiftDropThreshold):
                session.MarkStarted();
                logger.LogInformation(
                    "Session {Id}: Glass lifted — timer started (avg={Avg:F0}, baseline={Base:F0})",
                    session.SessionId, session.CurrentAverage, session.BaselineWeight);

                await hubContext.Clients.All.SendAsync("ChugStarted", new
                {
                    sessionId   = session.SessionId,
                    scaleNumber = session.ScaleNumber,
                    playerId    = session.PlayerId,
                    startTime   = session.StartTime,
                }, ct);
                break;

            // Glass lifted — detect return by tracking sustained weight above threshold.
            case ChugSessionState.Running when session.TrackReturn(
                _config.ReturnConfirmMinWeight, _config.ReturnConfirmReadings):

                session.MarkCompleted();
                logger.LogInformation(
                    "Session {Id}: Completed — duration={Duration}ms",
                    session.SessionId, session.DurationMs);

                await hubContext.Clients.All.SendAsync("ChugCompleted", new
                {
                    sessionId   = session.SessionId,
                    scaleNumber = session.ScaleNumber,
                    playerId    = session.PlayerId,
                    durationMs  = session.DurationMs,
                    startTime   = session.StartTime,
                    endTime     = session.EndTime,
                }, ct);
                break;
        }

        await SendUpdateAsync(session, ct);
    }

    private async Task SendUpdateAsync(ChugSession session, CancellationToken ct)
    {
        await hubContext.Clients.All.SendAsync("ChugUpdate", new
        {
            sessionId              = session.SessionId,
            scaleNumber            = session.ScaleNumber,
            playerId               = session.PlayerId,
            state                  = session.State.ToString(),
            currentAverage         = Math.Round(session.CurrentAverage, 1),
            baselineWeight         = session.BaselineWeight,
            elapsedMs              = session.ElapsedMs,
            isWaitingForBaseline   = session.State == ChugSessionState.WaitingForBaseline,
            isReadyToLift          = session.State == ChugSessionState.ReadyToLift,
            isRunning              = session.State == ChugSessionState.Running,
            isCompleted            = session.State == ChugSessionState.Completed,
            durationMs             = session.DurationMs,
        }, ct);
    }
}

/// <summary>Result of a <see cref="ChugService.ConfirmBaseline"/> call.</summary>
public record ConfirmBaselineResult(bool Success, string? Message, decimal? BaselineWeight, string State)
{
    public static ConfirmBaselineResult Fail(string message)
        => new(false, message, null, ChugSessionState.WaitingForBaseline.ToString());

    public static ConfirmBaselineResult Ok(decimal baselineWeight, string state)
        => new(true, null, baselineWeight, state);
}
