namespace _684BakOMeter.Web.Services;

/// <summary>State machine states for a manual-baseline chug session.</summary>
public enum ChugSessionState
{
    /// <summary>Waiting for the player to place a full glass and confirm with SPACE.</summary>
    WaitingForBaseline,

    /// <summary>Baseline weight captured — waiting for the player to lift the glass.</summary>
    ReadyToLift,

    /// <summary>Glass lifted — timer is running.</summary>
    Running,

    /// <summary>Glass returned and confirmed — chug completed.</summary>
    Completed,
}
