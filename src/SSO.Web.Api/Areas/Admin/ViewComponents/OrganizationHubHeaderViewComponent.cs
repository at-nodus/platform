using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity._Shared;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Admin.ViewComponents
{
	public sealed class OrganizationHubHeaderViewComponent : ViewComponent
	{
		private readonly IIdentityDbContextReader _reader;

		public OrganizationHubHeaderViewComponent(IIdentityDbContextReader reader)
		{
			_reader = reader;
		}

		public async Task<IViewComponentResult> InvokeAsync(Guid organizationId)
		{
			var org = await _reader.Query<Organization>().AsNoTracking()
				.FirstOrDefaultAsync(x => x.Id == organizationId && !x.IsDeleted);

			var branchCount = org is null
				? 0
				: await _reader.Query<SSO.Core.Domain.Identity.Branches.Entity.Branch>().AsNoTracking()
					.CountAsync(x => !x.IsDeleted && x.OrganizationId == organizationId);

			return View(new OrganizationHubHeaderModel
			{
				Organization = org,
				BranchCount = branchCount
			});
		}
	}

	public sealed class OrganizationHubHeaderModel
	{
		public Organization? Organization { get; set; }
		public int BranchCount { get; set; }
	}
}
