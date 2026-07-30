using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SSO.Middleware.Identity
{
	/// <summary>
	/// Cookie auth for Area /Me (self-service). Does not require sso.admin.*.
	/// Enriches portal context when available so CTAs can gate on permissions.
	/// </summary>
	public sealed class MePortalPageFilter : IAsyncPageFilter
	{
		private readonly IAdminPortalContextService _portalContext;

		public MePortalPageFilter(IAdminPortalContextService portalContext)
		{
			_portalContext = portalContext;
		}

		public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
			=> Task.CompletedTask;

		public async Task OnPageHandlerExecutionAsync(
			PageHandlerExecutingContext context,
			PageHandlerExecutionDelegate next)
		{
			if (!IsMeArea(context))
			{
				await next();
				return;
			}

			if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
			{
				context.Result = new ChallengeResult();
				return;
			}

			await _portalContext.EnsureEnrichedAsync(context.HttpContext.RequestAborted);
			await next();
		}

		private static bool IsMeArea(PageHandlerExecutingContext context)
		{
			if (context.ActionDescriptor is PageActionDescriptor page)
			{
				return string.Equals(page.AreaName, "Me", System.StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}
	}

	public abstract class MePageModel : PageModel
	{
		public IAdminPortalContextService Portal { get; }

		public string? Message { get; set; }
		public string? Error { get; set; }

		protected MePageModel(IAdminPortalContextService portal)
		{
			Portal = portal;
		}

		protected bool Can(string permission) => Portal.HasPermission(permission);

		protected bool ApplyResponse(ModelWrapper.WrapResponse response, string successMessage)
		{
			if (AdminWrap.IsSuccess(response))
			{
				Message = successMessage;
				return true;
			}

			Error = AdminWrap.ErrorMessage(response);
			return false;
		}
	}
}
