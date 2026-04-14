namespace _684BakOMeter.Web.ViewModels;

/// <summary>Powers the dashboard / home page.</summary>
public class DashboardViewModel
{
    public int TotalPlayers { get; set; }
    public int TotalAttempts { get; set; }

    /// <summary>The most recent attempts (latest 5).</summary>
    public IEnumerable<ChugAttemptListItemViewModel> LatestAttempts { get; set; }
        = Enumerable.Empty<ChugAttemptListItemViewModel>();

    /// <summary>The single fastest attempt recorded, or null if none exist.</summary>
    public ChugAttemptListItemViewModel? FastestAttempt { get; set; }
}
