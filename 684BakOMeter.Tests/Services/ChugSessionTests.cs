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

    [Fact]
    public void NewSession_StartsInWaitingForFull()
    {
        var session = CreateSession();
        Assert.Equal(ChugSessionState.WaitingForFull, session.State);
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
        for (int i = 0; i < ChugSession.AverageWindow; i++)
            session.AddValue(100m);
        Assert.True(session.HasEnoughValues);
    }

    // --- Validation buffer ---

    [Fact]
    public void AddValidationValue_IgnoresFirstSpikeReadings()
    {
        var session = CreateSession();
        // First ImpactSpikeIgnoreCount values are discarded
        for (int i = 0; i < ChugSession.ImpactSpikeIgnoreCount; i++)
            session.AddValidationValue(99_000m); // spike values

        Assert.Null(session.SettledValidationAverage);
    }

    [Fact]
    public void AddValidationValue_CollectsAfterSpikePeriod()
    {
        var session = CreateSession();
        for (int i = 0; i < ChugSession.ImpactSpikeIgnoreCount; i++)
            session.AddValidationValue(99_000m);

        session.AddValidationValue(65_000m);
        session.AddValidationValue(66_000m);

        Assert.Equal(65_500m, session.SettledValidationAverage);
    }

    [Fact]
    public void ValidationReadingCount_IncludesSpikeAndSettled()
    {
        var session = CreateSession();
        for (int i = 0; i < ChugSession.ImpactSpikeIgnoreCount; i++)
            session.AddValidationValue(99_000m);
        session.AddValidationValue(65_000m);

        Assert.Equal(ChugSession.ImpactSpikeIgnoreCount + 1, session.ValidationReadingCount);
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

    // --- FreezeEndTime ---

    [Fact]
    public void FreezeEndTime_DoesNotChangeState()
    {
        var session = CreateSession();
        session.MarkStarted();
        session.State = ChugSessionState.Validating;
        session.FreezeEndTime();

        Assert.Equal(ChugSessionState.Validating, session.State);
    }

    [Fact]
    public void FreezeEndTime_SetsDurationMs()
    {
        var session = CreateSession();
        session.MarkStarted();
        Thread.Sleep(50);
        session.FreezeEndTime();

        Assert.NotNull(session.DurationMs);
        Assert.True(session.DurationMs > 0);
    }

    [Fact]
    public void FreezeEndTime_FreezesElapsedMs()
    {
        var session = CreateSession();
        session.MarkStarted();
        Thread.Sleep(50);
        session.State = ChugSessionState.Validating;
        session.FreezeEndTime();

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

    // --- MarkInvalid ---

    [Fact]
    public void MarkInvalid_SetsStateToInvalid()
    {
        var session = CreateSession();
        session.MarkStarted();
        session.MarkInvalid();
        Assert.Equal(ChugSessionState.Invalid, session.State);
    }

    [Fact]
    public void MarkInvalid_SetsEndTime()
    {
        var session = CreateSession();
        session.MarkStarted();
        session.MarkInvalid();
        Assert.NotNull(session.EndTime);
    }
}
