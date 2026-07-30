using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Pages.Account
{
	[AllowAnonymous]
	public sealed class LogoutModel : PageModel
	{
		private readonly SignInManager<User> _signInManager;
		private readonly IAdminPortalContextService _portal;

		public LogoutModel(SignInManager<User> signInManager, IAdminPortalContextService portal)
		{
			_signInManager = signInManager;
			_portal = portal;
		}

		public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
		{
			await _portal.ClearContextAsync();
			await _signInManager.SignOutAsync();
			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return LocalRedirect(returnUrl);
			}

			return RedirectToPage("./Login");
		}

		public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
		{
			await _portal.ClearContextAsync();
			await _signInManager.SignOutAsync();
			if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			{
				return LocalRedirect(returnUrl);
			}

			return RedirectToPage("./Login");
		}
	}
}
