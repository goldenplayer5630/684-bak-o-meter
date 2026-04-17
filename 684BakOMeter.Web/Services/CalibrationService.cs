using System.Text.Json;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Reads and writes <see cref="CalibrationConfig"/> to a local JSON file.
/// Registered as a singleton so the cached config is shared across the app.
/// </summary>
public class CalibrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _filePath;
    private readonly ILogger<CalibrationService> _logger;
    private readonly Lock _lock = new();
    private CalibrationConfig? _cached;

    public CalibrationService(IWebHostEnvironment env, ILogger<CalibrationService> logger)
    {
        _logger = logger;
        _filePath = Path.Combine(env.ContentRootPath, "calibration.json");
    }

    /// <summary>Returns the current calibration config, or null if not yet calibrated.</summary>
    public CalibrationConfig? Load()
    {
        lock (_lock)
        {
            if (_cached is not null) return _cached;

            if (!File.Exists(_filePath))
                return null;

            try
            {
                var json = File.ReadAllText(_filePath);
                _cached = JsonSerializer.Deserialize<CalibrationConfig>(json, JsonOptions);
                return _cached;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read calibration file at {Path}", _filePath);
                return null;
            }
        }
    }

    /// <summary>Saves calibration values to disk and updates the in-memory cache.</summary>
    public void Save(CalibrationConfig config)
    {
        lock (_lock)
        {
            config.LastUpdatedUtc = DateTime.UtcNow;
            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(_filePath, json);
            _cached = config;
            _logger.LogInformation("Calibration saved to {Path}", _filePath);
        }
    }

    /// <summary>
    /// Builds a <see cref="ChugSessionConfig"/> from the stored calibration,
    /// using glass or pul thresholds based on the chug type.
    /// Falls back to default values when no calibration exists.
    /// </summary>
    public ChugSessionConfig BuildSessionConfig(Domain.Entities.ChugType chugType)
    {
        var cal = Load();
        if (cal is null)
            return new ChugSessionConfig();

        bool isPul = chugType == Domain.Entities.ChugType.Pul;

        decimal emptyVal = isPul ? cal.EmptyPul : cal.EmptyGlass;
        decimal fullVal = isPul ? cal.FullPul : cal.FullGlass;
        decimal nothingVal = cal.Nothing;

        // EmptyThreshold: midpoint between nothing and the empty container
        decimal emptyThreshold = (nothingVal + emptyVal) / 2m;
        // FullThreshold: midpoint between empty and full container
        decimal fullThreshold = (emptyVal + fullVal) / 2m;

        return new ChugSessionConfig
        {
            EmptyThreshold = emptyThreshold,
            FullThreshold = fullThreshold,
        };
    }
}
