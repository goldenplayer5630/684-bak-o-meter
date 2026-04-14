using System.ComponentModel.DataAnnotations;
using _684BakOMeter.Web.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace _684BakOMeter.Web.ViewModels;

/// <summary>Bound to the Create Chug Attempt form.</summary>
public class CreateChugAttemptViewModel
{
    [Required(ErrorMessage = "Please select a player.")]
    [Display(Name = "Player")]
    public int PlayerId { get; set; }

    [Required]
    [Display(Name = "Started At (UTC)")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Display(Name = "Ended At (UTC)")]
    public DateTime EndedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Duration must be 0 or greater.")]
    [Display(Name = "Duration (ms)")]
    public int DurationMs { get; set; }

    [Required]
    [Display(Name = "Chug Type")]
    public ChugType ChugType { get; set; } = ChugType.Bak;

    [StringLength(500)]
    [Display(Name = "Notes (optional)")]
    public string? Notes { get; set; }

    // Populated by the page handler for the Player dropdown
    public List<SelectListItem> PlayerOptions { get; set; } = new();

    // Populated by the page handler for the ChugType dropdown
    public List<SelectListItem> ChugTypeOptions { get; set; } = new();
}
