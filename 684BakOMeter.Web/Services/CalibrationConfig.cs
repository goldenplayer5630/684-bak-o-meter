namespace _684BakOMeter.Web.Services;

/// <summary>
/// Persisted calibration values captured from the load cell sensor.
/// Each value is a raw decimal reading from the HX711 scale.
/// </summary>
public class CalibrationConfig
{
    public decimal Nothing { get; set; }
    public decimal EmptyGlass { get; set; }
    public decimal FullGlass { get; set; }
    public decimal EmptyPul { get; set; }
    public decimal FullPul { get; set; }
    public DateTime? LastUpdatedUtc { get; set; }
}
