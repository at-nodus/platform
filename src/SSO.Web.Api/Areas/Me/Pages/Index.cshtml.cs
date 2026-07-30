using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Me.Pages
{
	public sealed class IndexModel : MePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IIdentityDbContextReader _reader;

		public IndexModel(
			IAdminPortalContextService portal,
			UserManager<User> userManager,
			IIdentityDbContextReader reader) : base(portal)
		{
			_userManager = userManager;
			_reader = reader;
		}

		public int OrganizationCount { get; set; }
		public int ProductCount { get; set; }
		public int RoleCount { get; set; }
		public int PendingInviteCount { get; set; }
		public string DisplayName { get; set; } = "Usuário";

		public async Task<IActionResult> OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			DisplayName = string.IsNullOrWhiteSpace(user.DisplayName)
				? user.UserName ?? "Usuário"
				: user.DisplayName;

			OrganizationCount = await _reader.Query<Membership>().AsNoTracking()
				.CountAsync(x => !x.IsDeleted && x.UserId == user.Id);

			var assignments = _reader.Query<UserRoleAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.UserId == user.Id);
			ProductCount = await assignments.Select(x => x.ProductId).Distinct().CountAsync();
			RoleCount = await assignments.Select(x => x.RoleId).Distinct().CountAsync();

			var email = user.Email ?? string.Empty;
			PendingInviteCount = await _reader.Query<OrganizationInvite>().AsNoTracking()
				.CountAsync(x => !x.IsDeleted
					&& x.Email == email
					&& x.Status == OrganizationInviteStatuses.Pending
					&& x.ExpiresAt > System.DateTime.UtcNow);

			return Page();
		}
	}
}
