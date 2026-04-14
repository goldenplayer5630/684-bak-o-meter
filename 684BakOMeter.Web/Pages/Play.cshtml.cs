using System.Text.Json;
using _684BakOMeter.Web.Domain.Enums;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _684BakOMeter.Web.Pages;

public class PlayModel : PageModel
{
    public string PlayPageDataJson { get; set; } = "{}";

    public void OnGet(string? type)
    {
        var slug = "Bak";
        var label = "Bak";

        if (!string.IsNullOrWhiteSpace(type) &&
            Enum.TryParse<ChugType>(type, ignoreCase: true, out var chugType))
        {
            slug = chugType.ToString();
            label = ChugTypeLabels.GetLabel(chugType);
        }

        PlayPageDataJson = JsonSerializer.Serialize(new
        {
            chugTypeSlug = slug,
            chugTypeLabel = label,
        });
    }
}
