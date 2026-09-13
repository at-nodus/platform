using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Organizations
{
	public sealed class LdapMapsModel : AdminHubPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IdentityDbContext _db;

		public LdapMapsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IdentityDbContext db) : base(portal)
		{
			_reader = reader;
			_db = db;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		public List<LdapGroupRoleMap> Items { get; set; } = new();
		public List<Role> Roles { get; set; } = new();
		public List<Product> Products { get; set; } = new();
		public List<Branch> Branches { get; set; } = new();

		[BindProperty]
		public string GroupIdentifier { get; set; } = string.Empty;

		[BindProperty]
		public Guid RoleId { get; set; }

		[BindProperty]
		public Guid ProductId { get; set; }

		[BindProperty]
		public Guid? BranchId { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin || !CanAccessOrganization(Id))
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!Portal.IsPlatformAdmin || !CanAccessOrganization(Id))
			{
				return Forbid();
			}

			try
			{
				var map = LdapGroupRoleMap.Create(Id, GroupIdentifier, RoleId, ProductId, BranchId);
				_db.LdapGroupRoleMaps.Add(map);
				await _db.SaveChangesAsync();
				Message = "Mapeamento criado.";
				GroupIdentifier = string.Empty;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid mapId)
		{
			if (!Portal.IsPlatformAdmin || !CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var map = await _db.LdapGroupRoleMaps.FirstOrDefaultAsync(x => x.Id == mapId && !x.IsDeleted && x.OrganizationId == Id);
			if (map is null)
			{
				Error = "Mapeamento não encontrado.";
			}
			else
			{
				map.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Mapeamento removido.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Roles = await _reader.Query<Role>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Products = await _reader.Query<Product>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Branches = PartyAddressFormatting.OrderBranchesMatrizThenTaxId(
					await _reader.Query<Branch>().AsNoTracking()
						.Where(x => !x.IsDeleted && x.OrganizationId == Id)
						.ToListAsync(),
					x => x.ParentBranchId,
					x => x.TaxId,
					x => x.Name)
				.ToList();
			Items = await _reader.Query<LdapGroupRoleMap>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == Id)
				.OrderBy(x => x.GroupIdentifier)
				.ToListAsync();
		}
	}
}
