using System;
using Microsoft.AspNetCore.Mvc;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin
{
	public abstract class AdminHubPageModel : AdminPageModel
	{
		protected AdminHubPageModel(IAdminPortalContextService portal) : base(portal)
		{
		}

		protected bool CanManageOrg => Portal.IsPlatformAdmin || Portal.HasPermission(SsoAdminPermissions.Org);

		protected bool CanAccessOrganization(Guid organizationId)
		{
			if (Portal.IsPlatformAdmin)
			{
				return true;
			}

			return CanManageOrg && Portal.OrganizationId == organizationId;
		}

		protected IActionResult RedirectToOrganizationHub(Guid? organizationId, string tab)
		{
			if (organizationId is Guid id)
			{
				return LocalRedirect(AdminHubUrls.Organization(id, tab));
			}

			if (Portal.IsPlatformAdmin)
			{
				return RedirectToPage("/Organizations");
			}

			return RedirectToPage("/SwitchContext");
		}

		protected IActionResult RedirectToContextOrganization(string tab)
			=> RedirectToOrganizationHub(Portal.OrganizationId, tab);
	}
}
