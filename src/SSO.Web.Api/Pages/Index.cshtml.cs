using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SSO.Web.Api.Pages
{
	public sealed class IndexModel : PageModel
	{
		public IActionResult OnGet()
		{
			if (User.Identity?.IsAuthenticated == true)
			{
				return Redirect("/Me");
			}

			return Redirect("/Account/Login");
		}
	}
}
