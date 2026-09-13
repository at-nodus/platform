using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Organizations
{
	public sealed class UserClaimsModel : AdminHubPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IdentityDbContext _db;

		public UserClaimsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IdentityDbContext db) : base(portal)
		{
			_reader = reader;
			_db = db;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		public List<UserClaimAssignment> Items { get; set; } = new();
		public List<ClaimDefinition> Definitions { get; set; } = new();
		public List<Product> Products { get; set; } = new();
		public List<Branch> Branches { get; set; } = new();
		public List<MemberOption> Members { get; set; } = new();

		[BindProperty]
		public Guid UserId { get; set; }

		[BindProperty]
		public Guid ClaimDefinitionId { get; set; }

		[BindProperty]
		public string Value { get; set; } = string.Empty;

		[BindProperty]
		public Guid ProductId { get; set; }

		[BindProperty]
		public Guid? BranchId { get; set; }

		[BindProperty]
		public bool Inheritable { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanAccessOrganization(Id))
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var definition = await _db.ClaimDefinitions.AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == ClaimDefinitionId);
			if (definition is null)
			{
				Error = "Definição de claim não encontrada.";
			}
			else if (!ClaimValueTypes.TryValidate(definition.ValueType, Value, out var typeError))
			{
				Error = typeError;
			}
			else
			{
				try
				{
					var entity = UserClaimAssignment.Create(UserId, ClaimDefinitionId, Value, ProductId, Id, BranchId, Inheritable);
					_db.UserClaimAssignments.Add(entity);
					await _db.SaveChangesAsync();
					Message = "Claim atribuído ao usuário.";
					Value = string.Empty;
				}
				catch (Exception ex)
				{
					Error = ex.Message;
				}
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid assignmentId)
		{
			if (!CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var entity = await _db.UserClaimAssignments.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == assignmentId && x.OrganizationId == Id);
			if (entity is null)
			{
				Error = "Atribuição não encontrada.";
			}
			else
			{
				entity.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Atribuição removida.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Definitions = await _reader.Query<ClaimDefinition>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Products = await _reader.Query<Product>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Branches = PartyAddressFormatting.OrderBranchesMatrizThenTaxId(
					await _reader.Query<Branch>().AsNoTracking()
						.Where(x => !x.IsDeleted && x.OrganizationId == Id)
						.ToListAsync(),
					x => x.ParentBranchId,
					x => x.TaxId,
					x => x.Name)
				.ToList();
			Members = await (
				from m in _reader.Query<Membership>().AsNoTracking()
				join u in _reader.Query<User>().AsNoTracking() on m.UserId equals u.Id
				where !m.IsDeleted && !u.IsDeleted && m.OrganizationId == Id
				orderby u.Email
				select new MemberOption { Id = u.Id, Label = u.Email ?? u.UserName ?? u.Id.ToString() }).ToListAsync();
			Items = await _reader.Query<UserClaimAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == Id)
				.OrderByDescending(x => x.CreatedAt)
				.Take(200)
				.ToListAsync();
		}

		public sealed class MemberOption
		{
			public Guid Id { get; set; }
			public string Label { get; set; } = string.Empty;
		}
	}
}
