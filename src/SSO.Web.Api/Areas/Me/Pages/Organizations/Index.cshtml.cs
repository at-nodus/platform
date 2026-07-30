using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Me.Pages.Organizations
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

		public List<Organization> Items { get; set; } = new();
		public Dictionary<Guid, int> BranchCounts { get; set; } = new();
		public bool CanManageOrgs => Portal.IsPlatformAdmin || Portal.HasPermission(SsoAdminPermissions.Org);

		[BindProperty(SupportsGet = true)]
		public string? Search { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			if (Portal.IsPlatformAdmin)
			{
				Items = await _reader.Query<Organization>().AsNoTracking()
					.Where(x => !x.IsDeleted)
					.OrderBy(x => x.Name)
					.ToListAsync();
			}
			else
			{
				Items = await (
					from m in _reader.Query<Membership>().AsNoTracking()
					join o in _reader.Query<Organization>().AsNoTracking() on m.OrganizationId equals o.Id
					where !m.IsDeleted && !o.IsDeleted && m.UserId == user.Id
					orderby o.Name
					select o).ToListAsync();
			}

			if (!string.IsNullOrWhiteSpace(Search))
			{
				var term = Search.Trim();
				Items = Items
					.Where(x => x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
						|| x.Code.Contains(term, StringComparison.OrdinalIgnoreCase))
					.ToList();
			}

			var ids = Items.Select(x => x.Id).ToList();
			BranchCounts = await _reader.Query<Branch>().AsNoTracking()
				.Where(x => !x.IsDeleted && ids.Contains(x.OrganizationId))
				.GroupBy(x => x.OrganizationId)
				.ToDictionaryAsync(x => x.Key, x => x.Count());

			return Page();
		}
	}
}
