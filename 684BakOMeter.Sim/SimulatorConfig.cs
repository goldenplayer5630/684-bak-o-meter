namespace _685BakOMeter.Sim;

/// <summary>
/// Configuration constants for the Bak-O-Meter hardware simulator.
/// Centralizes calibration values, randomization margins, and timing parameters.
/// </summary>
public static class SimulatorConfig
{
    #region Serial Configuration

    public const int DefaultBaudRate = 115200;
    public const string DefaultComPort = "COM20";

    #endregion

    #region Calibration Base Values

    /// <summary>Base load cell value when nothing is on the scale.</summary>
    public const int Nothing = 34413;

    /// <summary>Base load cell value for an empty glass.</summary>
    public const int EmptyGlass = 77670;

    /// <summary>Base load cell value for a full glass.</summary>
    public const int FullGlass = 92366;

    /// <summary>Base load cell value for an empty pul (small glass).</summary>
    public const int EmptyPul = 120680;

    /// <summary>Base load cell value for a full pul (small glass).</summary>
    public const int FullPul = 170882;

    #endregion

    #region Randomization Margins

    /// <summary>Random variance range (+/-) for "nothing" readings.</summary>
    public const int NothingMargin = 100;

    /// <summary>Random variance range (+/-) for empty glass readings.</summary>
    public const int EmptyGlassMargin = 150;

    /// <summary>Random variance range (+/-) for full glass readings.</summary>
    public const int FullGlassMargin = 200;

    /// <summary>Random variance range (+/-) for empty pul readings.</summary>
    public const int EmptyPulMargin = 80;

    /// <summary>Random variance range (+/-) for full pul readings.</summary>
    public const int FullPulMargin = 100;

    #endregion

    #region Chug Simulation Timing

    /// <summary>Minimum duration (ms) the glass/pul is removed during chug simulation.</summary>
    public const int RemovalMinMs = 1000;

    /// <summary>Maximum duration (ms) the glass/pul is removed during chug simulation.</summary>
    public const int RemovalMaxMs = 5000;

    /// <summary>Duration (ms) of the impact spike when glass is placed back hard.</summary>
    public const int ImpactDurationMs = 1000;

    /// <summary>How much to overshoot the target value during impact spike (multiplier).</summary>
    public const double ImpactOvershotMultiplier = 1.5;

    /// <summary>Interval (ms) between sending weight updates during chug simulation.</summary>
    public const int SimulationUpdateIntervalMs = 100;

    #endregion

    #region Simulated NFC Tags

    /// <summary>Realistic example NFC tag UIDs for testing.</summary>
    public static readonly string[] NfcTags = new[]
    {
        "04A1B2C3D4E5F0",
        "04B2C3D4E5F601",
        "04C3D4E5F60712",
        "04D4E5F6071823",
        "04E5F607182934",
        "04F6071829A345",
        "0407182934B456",
        "08182934C56789",
        "092934D678ABCD",
        "0A34E789CDEF12"
    };

    #endregion

    #region Protocol Format

    /// <summary>Prefix for RFID/NFC tag messages.</summary>
    public const string RfidPrefix = "RFID";

    /// <summary>Prefix for scale/load cell messages (using scale 1).</summary>
    public const string ScalePrefix = "SCALE1";

    /// <summary>
    /// Formats a protocol message in PREFIX:VALUE format.
    /// </summary>
    public static string FormatMessage(string prefix, string value)
    {
        return $"{prefix}:{value}";
    }

    #endregion
}
