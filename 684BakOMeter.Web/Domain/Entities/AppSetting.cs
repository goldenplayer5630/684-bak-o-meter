namespace _684BakOMeter.Web.Domain.Entities;

/// <summary>
/// Generic key-value settings row for application-wide configuration.
/// Only one row per key is allowed (Key is the primary key).
/// </summary>
public class AppSetting
{
    /// <summary>Unique setting key, e.g. <c>"ApplicationMode"</c>.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>String-encoded value for the setting.</summary>
    public string Value { get; set; } = string.Empty;
}
