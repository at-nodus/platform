using Microsoft.AspNetCore.Mvc;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class MyOrganizationModel : AdminHubPageModel
	{
		public MyOrganizationModel(IAdminPortalContextService portal) : base(portal)
		{
		}

		public IActionResult OnGet()
		{
			if (Portal.OrganizationId is Guid orgId)
			{
				return LocalRedirect(AdminHubUrls.OrganizationDetails(orgId));
			}

			return RedirectToPage("/SwitchContext");
		}
	}
}
