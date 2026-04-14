namespace _684BakOMeter.Web.ViewModels;

public class PlayerListItemViewModel
{
    public int    Id             { get; set; }
    public string Name           { get; set; } = string.Empty;
    public int    AttemptCount   { get; set; }
    public int?   BestDurationMs { get; set; }

    public string? BestDurationDisplay =>
        BestDurationMs.HasValue ? $"{BestDurationMs.Value / 1000.0:F2} s" : null;
}
