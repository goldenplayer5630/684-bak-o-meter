using _684BakOMeter.Web.Data.Persistence;
using _684BakOMeter.Web.Data.Persistence.EntityConfigurations;
using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Reads and writes the active <see cref="ApplicationMode"/> from the
/// <c>AppSettings</c> table. Inject as a scoped service so each request
/// gets a fresh read from the database.
/// </summary>
public class AppModeService
{
    private readonly AppDbContext _db;

    public AppModeService(AppDbContext db) => _db = db;

    /// <summary>Returns the current application mode. Defaults to <see cref="ApplicationMode.Official"/> if the row is missing.</summary>
    public async Task<ApplicationMode> GetCurrentModeAsync()
    {
        var setting = await _db.AppSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == AppSettingConfig.ApplicationModeKey);

        if (setting is null) return ApplicationMode.Official;

        return Enum.TryParse<ApplicationMode>(setting.Value, ignoreCase: true, out var mode)
            ? mode
            : ApplicationMode.Official;
    }

    /// <summary>Persists a new application mode, upserting the settings row.</summary>
    public async Task SetModeAsync(ApplicationMode mode)
    {
        var setting = await _db.AppSettings
            .FirstOrDefaultAsync(s => s.Key == AppSettingConfig.ApplicationModeKey);

        if (setting is null)
        {
            _db.AppSettings.Add(new AppSetting
            {
                Key   = AppSettingConfig.ApplicationModeKey,
                Value = mode.ToString(),
            });
        }
        else
        {
            setting.Value = mode.ToString();
        }

        await _db.SaveChangesAsync();
    }
}
