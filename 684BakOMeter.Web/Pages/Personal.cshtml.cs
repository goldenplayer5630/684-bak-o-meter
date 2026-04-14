using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _684BakOMeter.Web.Pages;

/// <summary>
/// Page model for the personal stats page.
/// All data is fetched client-side via GET /api/personal?name=...
/// so no server-side work is needed here.
/// </summary>
public class PersonalModel : PageModel
{
    public void OnGet() { }
}
