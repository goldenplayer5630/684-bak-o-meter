using System.ComponentModel.DataAnnotations;

namespace _684BakOMeter.Web.ViewModels;

public class CreatePlayerViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Player Name")]
    public string Name { get; set; } = string.Empty;
}
