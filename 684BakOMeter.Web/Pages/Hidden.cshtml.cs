using System.Text.Json;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _684BakOMeter.Web.Pages;

public class HiddenMenuModel : PageModel
{
    public string ChugTypesJson { get; set; } = "[]";

    public void OnGet()
    {
        var types = ChugTypeLabels.Hidden.Select(kv => new
        {
            slug = kv.Key.ToString(),
            label = kv.Value,
        }).ToList();

        ChugTypesJson = JsonSerializer.Serialize(types);
    }
}
