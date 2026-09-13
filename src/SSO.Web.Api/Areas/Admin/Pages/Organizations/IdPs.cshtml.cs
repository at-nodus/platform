using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Organizations
{
	public sealed class IdPsModel : AdminHubPageModel
	{
		private readonly IdentityDbContext _db;

		public IdPsModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		public List<ExternalIdentityProvider> Items { get; set; } = new();

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin || !CanAccessOrganization(Id))
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostToggleEnabledAsync(Guid idpId)
		{
			if (!Portal.IsPlatformAdmin || !CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var idp = await _db.ExternalIdentityProviders.FirstOrDefaultAsync(x => x.Id == idpId && !x.IsDeleted && x.OrganizationId == Id);
			if (idp is null)
			{
				Error = "Provedor não encontrado.";
			}
			else
			{
				idp.IsEnabled = !idp.IsEnabled;
				idp.TouchUpdated();
				await _db.SaveChangesAsync();
				Message = $"Provedor {(idp.IsEnabled ? "habilitado" : "desabilitado")}.";
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostToggleJitAsync(Guid idpId)
		{
			if (!Portal.IsPlatformAdmin || !CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var idp = await _db.ExternalIdentityProviders.FirstOrDefaultAsync(x => x.Id == idpId && !x.IsDeleted && x.OrganizationId == Id);
			if (idp is null)
			{
				Error = "Provedor não encontrado.";
			}
			else
			{
				idp.AllowJitProvisioning = !idp.AllowJitProvisioning;
				idp.TouchUpdated();
				await _db.SaveChangesAsync();
				Message = $"JIT provisioning {(idp.AllowJitProvisioning ? "habilitado" : "desabilitado")}.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Items = await _db.ExternalIdentityProviders.AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == Id)
				.OrderBy(x => x.Code)
				.ToListAsync();
		}
	}
}
