using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Services;

namespace _684BakOMeter.Tests.Services;

public class ChugSessionTests
{
    private static ChugSession CreateSession() => new()
    {
        PlayerId = 1,
        ChugType = ChugType.Bak,
        ScaleNumber = 1,
    };

    /// <summary>Fills the rolling window with <paramref name="count"/> identical values.</summary>
    private static void Fill(ChugSession session, decimal value, int count = ChugSession.AverageWindow)
    {
        for (int i = 0; i < count; i++)
            session.AddValue(value);
    }

    // --- Initial state ---

    [Fact]
    public void NewSession_StartsInWaitingForBaseline()
    {
        var session = CreateSession();
        Assert.Equal(ChugSessionState.WaitingForBaseline, session.State);
    }

    [Fact]
    public void SessionId_Is8Characters()
    {
        var session = CreateSession();
        Assert.Equal(8, session.SessionId.Length);
    }

    // --- AddValue / Rolling Average ---

    [Fact]
    public void AddValue_SingleValue_SetsAverage()
    {
        var session = CreateSession();
        session.AddValue(100m);
        Assert.Equal(100m, session.CurrentAverage);
    }

    [Fact]
    public void AddValue_TwoValues_ComputesAverage()
    {
        var session = CreateSession();
        session.AddValue(100m);
        session.AddValue(200m);
        Assert.Equal(150m, session.CurrentAverage);
    }

    [Fact]
    public void AddValue_ExceedsWindow_DropsOldest()
    {
        var session = CreateSession();
        session.AddValue(100m);
        session.AddValue(200m);
        session.AddValue(300m);
        session.AddValue(400m);
        session.AddValue(500m); // window=4, so 100 is dropped

        Assert.Equal(350m, session.CurrentAverage); // avg(200,300,400,500)
    }

    [Fact]
    public void HasEnoughValues_FalseBeforeWindowFilled()
    {
        var session = CreateSession();
        session.AddValue(100m);
        session.AddValue(200m);
        session.AddValue(300m);
        Assert.False(session.HasEnoughValues);
    }

    [Fact]
    public void HasEnoughValues_TrueWhenWindowFilled()
    {
        var session = CreateSession();
        Fill(session, 100m);
        Assert.True(session.HasEnoughValues);
    }

    [Fact]
    public void AddValue_TracksPreviousAverage()
    {
        var session = CreateSession();
        Fill(session, 100m);
        var before = session.CurrentAverage;
        session.AddValue(200m);
        Assert.Equal(before, session.PreviousAverage);
    }

    // --- CaptureBaseline ---

    [Fact]
    public void CaptureBaseline_SetsBaselineWeight()
    {
        var session = CreateSession();
        Fill(session, 80_000m);
        session.CaptureBaseline();
        Assert.Equal(80_000m, session.BaselineWeight);
    }

    [Fact]
    public void CaptureBaseline_TransitionsToReadyToLift()
    {
        var session = CreateSession();
        Fill(session, 80_000m);
        session.CaptureBaseline();
        Assert.Equal(ChugSessionState.ReadyToLift, session.State);
    }

    // --- IsLifted ---

    [Fact]
    public void IsLifted_FalseWhenNoBaseline()
    {
        var session = CreateSession();
        Fill(session, 5_000m);
        Assert.False(session.IsLifted(0.5m));
    }

    [Fact]
    public void IsLifted_FalseWhenWeightAboveDropThreshold()
    {
        var session = CreateSession();
        Fill(session, 80_000m);
        session.CaptureBaseline(); // baseline = 80_000
        Fill(session, 75_000m);   // 75k > 80k * 0.5 = 40k
        Assert.False(session.IsLifted(0.5m));
    }

    [Fact]
    public void IsLifted_TrueWhenWeightDropsBelowThreshold()
    {
        var session = CreateSession();
        Fill(session, 80_000m);
        session.CaptureBaseline(); // baseline = 80_000
        Fill(session, 30_000m);   // 30k < 80k * 0.5 = 40k
        Assert.True(session.IsLifted(0.5m));
    }

    // --- MarkStarted ---

    [Fact]
    public void MarkStarted_SetsStateToRunning()
    {
        var session = CreateSession();
        session.MarkStarted();
        Assert.Equal(ChugSessionState.Running, session.State);
    }

    [Fact]
    public void MarkStarted_SetsStartTime()
    {
        var session = CreateSession();
        var before = DateTime.UtcNow;
        session.MarkStarted();
        var after = DateTime.UtcNow;

        Assert.NotNull(session.StartTime);
        Assert.InRange(session.StartTime!.Value, before, after);
    }

    // --- TrackReturn ---

    [Fact]
    public void TrackReturn_ReturnsFalseBeforeThresholdReached()
    {
        var session = CreateSession();
        session.MarkStarted();
        Fill(session, 34_000m);
        session.AddValue(80_000m); // count=1, need 3
        bool result = session.TrackReturn(40_000m, 3);
        Assert.False(result);
    }

    [Fact]
    public void TrackReturn_ReturnsTrueAfterConsecutiveConfirmedReadings()
    {
        var session = CreateSession();
        session.MarkStarted();
        Fill(session, 34_000m);
        bool result = false;
        for (int i = 0; i < 3; i++)
        {
            session.AddValue(80_000m);
            result = session.TrackReturn(40_000m, 3);
        }
        Assert.True(result);
    }

    [Fact]
    public void TrackReturn_ResetsCounterOnLowReading()
    {
        var session = CreateSession();
        session.MarkStarted();
        Fill(session, 34_000m);
        session.AddValue(80_000m);
        session.TrackReturn(40_000m, 3); // count=1
        Fill(session, 34_000m);          // avg drops below threshold
        session.TrackReturn(40_000m, 3); // count reset to 0
        session.AddValue(80_000m);
        bool result = session.TrackReturn(40_000m, 3); // count=1 — not enough
        Assert.False(result);
    }

    // --- MarkCompleted ---

    [Fact]
    public void MarkCompleted_SetsStateToCompleted()
    {
        var session = CreateSession();
        session.MarkStarted();
        session.MarkCompleted();
        Assert.Equal(ChugSessionState.Completed, session.State);
    }

    [Fact]
    public void MarkCompleted_SetsEndTime()
    {
        var session = CreateSession();
        session.MarkStarted();
        session.MarkCompleted();
        Assert.NotNull(session.EndTime);
    }

    [Fact]
    public void MarkCompleted_SetsDurationMs()
    {
        var session = CreateSession();
        session.MarkStarted();
        Thread.Sleep(50);
        session.MarkCompleted();
        Assert.NotNull(session.DurationMs);
        Assert.True(session.DurationMs > 0);
    }

    // --- ElapsedMs ---

    [Fact]
    public void ElapsedMs_ZeroBeforeStarted()
    {
        var session = CreateSession();
        Assert.Equal(0, session.ElapsedMs);
    }

    [Fact]
    public void ElapsedMs_TicksWhileRunning()
    {
        var session = CreateSession();
        session.MarkStarted();
        Thread.Sleep(50);
        Assert.True(session.ElapsedMs > 0);
    }

    [Fact]
    public void ElapsedMs_FrozenAfterCompleted()
    {
        var session = CreateSession();
        session.MarkStarted();
        Thread.Sleep(50);
        session.MarkCompleted();
        var frozen = session.ElapsedMs;
        Thread.Sleep(50);
        Assert.Equal(frozen, session.ElapsedMs);
    }

    // --- DurationMs ---

    [Fact]
    public void DurationMs_NullBeforeStartAndEnd()
    {
        var session = CreateSession();
        Assert.Null(session.DurationMs);
    }

    [Fact]
    public void DurationMs_NullWhenOnlyStarted()
    {
        var session = CreateSession();
        session.MarkStarted();
        Assert.Null(session.DurationMs);
    }
}
