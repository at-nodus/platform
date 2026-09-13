using Microsoft.AspNetCore.Mvc;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class UserClaimAssignmentsModel : AdminHubPageModel
	{
		public UserClaimAssignmentsModel(IAdminPortalContextService portal) : base(portal)
		{
		}

		public IActionResult OnGet()
		{
			if (Portal.OrganizationId is Guid)
			{
				return RedirectToContextOrganization("userclaims");
			}

			return RedirectToPage("/Users");
		}
	}
}
