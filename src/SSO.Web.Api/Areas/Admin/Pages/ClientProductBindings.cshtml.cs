using Microsoft.AspNetCore.Mvc;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class ClientProductBindingsModel : AdminHubPageModel
	{
		public ClientProductBindingsModel(IAdminPortalContextService portal) : base(portal)
		{
		}

		public IActionResult OnGet() => RedirectToPage("/AuthClients");
	}
}
