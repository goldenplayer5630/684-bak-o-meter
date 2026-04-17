namespace _684BakOMeter.Web.Services;

/// <summary>State machine states for a chug session driven by scale measurements.</summary>
public enum ChugSessionState
{
    /// <summary>Waiting for the full glass to be placed on the scale.</summary>
    WaitingForFull,

    /// <summary>Full glass detected — ready for the player to lift it.</summary>
    Ready,

    /// <summary>Glass lifted — timer is running.</summary>
    Running,

    /// <summary>Empty glass placed back — chug completed.</summary>
    Completed,

    /// <summary>Full glass placed back without drinking — invalid attempt.</summary>
    Invalid
}
