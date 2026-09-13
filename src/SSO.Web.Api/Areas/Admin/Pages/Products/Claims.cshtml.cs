using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Products
{
	public sealed class ClaimsModel : AdminHubPageModel
	{
		private readonly IdentityDbContext _db;

		public ClaimsModel(IAdminPortalContextService portal, IdentityDbContext db) : base(portal)
		{
			_db = db;
		}

		public List<ClaimDefinition> Items { get; set; } = new();

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		[BindProperty]
		public string ValueType { get; set; } = ClaimValueTypes.String;

		[BindProperty]
		public string? Description { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			try
			{
				var entity = ClaimDefinition.Create(Code.Trim().ToLowerInvariant(), Name, ValueType, null, Description);
				_db.ClaimDefinitions.Add(entity);
				await _db.SaveChangesAsync();
				Message = "Definição de claim criada.";
				Code = Name = string.Empty;
				Description = null;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var entity = await _db.ClaimDefinitions.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id && x.ProductId == null);
			if (entity is null)
			{
				Error = "Definição não encontrada.";
			}
			else
			{
				entity.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Definição removida.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Items = await _db.ClaimDefinitions.AsNoTracking()
				.Where(x => !x.IsDeleted && x.ProductId == null)
				.OrderBy(x => x.Code)
				.ToListAsync();
		}
	}
}
