using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Store.Pages
{
    // restrict access
    [Authorize(Roles="Admins")]
    public class AdminPageModel : PageModel
    {
    }
}
