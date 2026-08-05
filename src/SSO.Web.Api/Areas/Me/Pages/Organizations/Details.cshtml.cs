using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Branches.Commands;
using SSO.Core.Application.Identity.OrganizationContacts.Commands;
using SSO.Core.Application.Identity.Organizations.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Me.Pages.Organizations
{
	public sealed class DetailsModel : MePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public DetailsModel(
			IAdminPortalContextService portal,
			UserManager<User> userManager,
			IIdentityDbContextReader reader,
			IMediator mediator) : base(portal)
		{
			_userManager = userManager;
			_reader = reader;
			_mediator = mediator;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		public Organization? Organization { get; set; }
		public List<Branch> Branches { get; set; } = new();
		public List<OrganizationContact> Contacts { get; set; } = new();
		public List<ProductRow> Products { get; set; } = new();
		public List<MemberRow> Members { get; set; } = new();

		public bool CanEditOrg => Portal.IsPlatformAdmin;
		public bool CanManageBranches => Portal.IsPlatformAdmin || Portal.HasPermission(SsoAdminPermissions.Org);
		public bool CanManageContacts => Portal.IsPlatformAdmin || Portal.HasPermission(SsoAdminPermissions.Org);
		public bool CanViewMembers => Portal.IsPlatformAdmin || Portal.HasPermission(SsoAdminPermissions.Org);
		public bool IsAdminRoute => Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase);

		/// <summary>Tab ativa após postback (dados|branches|contato|produtos|usuarios).</summary>
		public string ActiveTab { get; set; } = "dados";

		[BindProperty]
		public string OrgName { get; set; } = string.Empty;

		[BindProperty]
		public string OrgCode { get; set; } = string.Empty;

		[BindProperty]
		public string? OrgLegalName { get; set; }

		[BindProperty]
		public string? OrgTradeName { get; set; }

		[BindProperty]
		public string? OrgTaxId { get; set; }

		[BindProperty]
		public string? OrgSegment { get; set; }

		[BindProperty]
		public string? OrgDescription { get; set; }

		[BindProperty]
		public string? OrgPostalCode { get; set; }

		[BindProperty]
		public string? OrgStreet { get; set; }

		[BindProperty]
		public string? OrgNumber { get; set; }

		[BindProperty]
		public string? OrgComplement { get; set; }

		[BindProperty]
		public string? OrgCity { get; set; }

		[BindProperty]
		public string? OrgState { get; set; }

		[BindProperty]
		public string BranchName { get; set; } = string.Empty;

		[BindProperty]
		public string BranchCode { get; set; } = string.Empty;

		[BindProperty]
		public string? BranchLegalName { get; set; }

		[BindProperty]
		public string? BranchTradeName { get; set; }

		[BindProperty]
		public string? BranchTaxId { get; set; }

		[BindProperty]
		public string? BranchSegment { get; set; }

		[BindProperty]
		public string? BranchDescription { get; set; }

		[BindProperty]
		public string? BranchPostalCode { get; set; }

		[BindProperty]
		public string? BranchStreet { get; set; }

		[BindProperty]
		public string? BranchNumber { get; set; }

		[BindProperty]
		public string? BranchComplement { get; set; }

		[BindProperty]
		public string? BranchCity { get; set; }

		[BindProperty]
		public string? BranchState { get; set; }

		[BindProperty(SupportsGet = true)]
		public Guid? BranchEditId { get; set; }

		[BindProperty]
		public string ContactName { get; set; } = string.Empty;

		[BindProperty]
		public string? ContactEmail { get; set; }

		[BindProperty]
		public string? ContactPhone { get; set; }

		[BindProperty]
		public string? ContactTitle { get; set; }

		[BindProperty]
		public bool ContactIsPrimary { get; set; }

		[BindProperty(SupportsGet = true)]
		public Guid? ContactEditId { get; set; }

		public sealed class ProductRow
		{
			public string Name { get; set; } = string.Empty;
			public string Code { get; set; } = string.Empty;
		}

		public sealed class MemberRow
		{
			public string Email { get; set; } = string.Empty;
			public string? DisplayName { get; set; }
			public List<string> Roles { get; set; } = new();
		}

		public async Task<IActionResult> OnGetAsync()
		{
			if (!await EnsureCanViewAsync())
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostUpdateOrgAsync()
		{
			if (!CanEditOrg || !await EnsureCanViewAsync())
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PutOrganizationCommand>(new
			{
				id = Id,
				name = OrgName,
				code = OrgCode,
				legalName = EmptyToNull(OrgLegalName),
				tradeName = EmptyToNull(OrgTradeName),
				taxId = EmptyToNull(OrgTaxId),
				segment = EmptyToNull(OrgSegment),
				description = EmptyToNull(OrgDescription),
				postalCode = EmptyToNull(OrgPostalCode),
				street = EmptyToNull(OrgStreet),
				number = EmptyToNull(OrgNumber),
				complement = EmptyToNull(OrgComplement),
				city = EmptyToNull(OrgCity),
				state = EmptyToNull(OrgState)?.ToUpperInvariant(),
				branchAuthzInheritance = Organization?.BranchAuthzInheritance ?? BranchAuthzInheritancePolicies.Off
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Empresa atualizada.");
			await LoadAsync();
			ActiveTab = "dados";
			return Page();
		}

		public async Task<IActionResult> OnPostCreateBranchAsync()
		{
			if (!CanManageBranches || !await EnsureCanViewAsync())
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PostBranchCommand>(new
			{
				organizationId = Id,
				name = BranchName,
				code = BranchCode,
				legalName = EmptyToNull(BranchLegalName),
				tradeName = EmptyToNull(BranchTradeName),
				taxId = EmptyToNull(BranchTaxId),
				segment = EmptyToNull(BranchSegment),
				description = EmptyToNull(BranchDescription),
				postalCode = EmptyToNull(BranchPostalCode),
				street = EmptyToNull(BranchStreet),
				number = EmptyToNull(BranchNumber),
				complement = EmptyToNull(BranchComplement),
				city = EmptyToNull(BranchCity),
				state = EmptyToNull(BranchState)?.ToUpperInvariant()
			});
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Filial criada."))
			{
				ClearBranchForm();
			}

			await LoadAsync();
			ActiveTab = "branches";
			return Page();
		}

		public async Task<IActionResult> OnPostUpdateBranchAsync()
		{
			if (!CanManageBranches || BranchEditId is not Guid branchId || !await EnsureCanViewAsync())
			{
				return Forbid();
			}

			var existing = await _reader.Query<Branch>().AsNoTracking()
				.FirstOrDefaultAsync(x => x.Id == branchId && x.OrganizationId == Id && !x.IsDeleted);
			if (existing is null)
			{
				Error = "Filial não encontrada.";
				await LoadAsync();
				return Page();
			}

			var cmd = AdminWrap.FromAnonymous<PutBranchCommand>(new
			{
				id = branchId,
				organizationId = Id,
				name = BranchName,
				code = BranchCode,
				parentBranchId = existing.ParentBranchId,
				legalName = EmptyToNull(BranchLegalName),
				tradeName = EmptyToNull(BranchTradeName),
				taxId = EmptyToNull(BranchTaxId),
				segment = EmptyToNull(BranchSegment),
				description = EmptyToNull(BranchDescription),
				postalCode = EmptyToNull(BranchPostalCode),
				street = EmptyToNull(BranchStreet),
				number = EmptyToNull(BranchNumber),
				complement = EmptyToNull(BranchComplement),
				city = EmptyToNull(BranchCity),
				state = EmptyToNull(BranchState)?.ToUpperInvariant()
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Filial atualizada.");
			BranchEditId = null;
			ClearBranchForm();
			await LoadAsync();
			ActiveTab = "branches";
			return Page();
		}

		public async Task<IActionResult> OnPostSaveContactAsync()
		{
			if (!CanManageContacts || !await EnsureCanViewAsync())
			{
				return Forbid();
			}

			if (ContactEditId is Guid contactId)
			{
				var cmd = AdminWrap.FromAnonymous<PutOrganizationContactCommand>(new
				{
					id = contactId,
					organizationId = Id,
					name = ContactName,
					email = ContactEmail,
					phone = ContactPhone,
					title = ContactTitle,
					isPrimary = ContactIsPrimary
				});
				var response = await _mediator.Send(cmd);
				ApplyResponse(response, "Contato atualizado.");
			}
			else
			{
				var cmd = AdminWrap.FromAnonymous<PostOrganizationContactCommand>(new
				{
					organizationId = Id,
					name = ContactName,
					email = ContactEmail,
					phone = ContactPhone,
					title = ContactTitle,
					isPrimary = ContactIsPrimary
				});
				var response = await _mediator.Send(cmd);
				ApplyResponse(response, "Contato criado.");
			}

			ContactEditId = null;
			ContactName = string.Empty;
			ContactEmail = null;
			ContactPhone = null;
			ContactTitle = null;
			ContactIsPrimary = false;
			await LoadAsync();
			ActiveTab = "contato";
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteContactAsync(Guid contactId)
		{
			if (!CanManageContacts || !await EnsureCanViewAsync())
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteOrganizationContactCommand>(new { id = contactId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Contato removido.");
			await LoadAsync();
			ActiveTab = "contato";
			return Page();
		}

		private async Task<bool> EnsureCanViewAsync()
		{
			if (Portal.IsPlatformAdmin)
			{
				return true;
			}

			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return false;
			}

			return await _reader.Query<Membership>().AsNoTracking()
				.AnyAsync(x => !x.IsDeleted && x.UserId == user.Id && x.OrganizationId == Id);
		}

		private async Task LoadAsync()
		{
			Organization = await _reader.Query<Organization>().AsNoTracking()
				.FirstOrDefaultAsync(x => x.Id == Id && !x.IsDeleted);

			if (Organization is null)
			{
				Error = "Empresa não encontrada.";
				return;
			}

			OrgName = Organization.Name;
			OrgCode = Organization.Code;
			OrgLegalName = Organization.LegalName;
			OrgTradeName = Organization.TradeName;
			OrgTaxId = Organization.TaxId;
			OrgSegment = Organization.Segment;
			OrgDescription = Organization.Description;
			OrgPostalCode = Organization.PostalCode;
			OrgStreet = Organization.Street;
			OrgNumber = Organization.Number;
			OrgComplement = Organization.Complement;
			OrgCity = Organization.City;
			OrgState = Organization.State;

			Branches = PartyAddressFormatting.OrderBranchesMatrizThenTaxId(
					await _reader.Query<Branch>().AsNoTracking()
						.Where(x => !x.IsDeleted && x.OrganizationId == Id)
						.ToListAsync(),
					x => x.ParentBranchId,
					x => x.TaxId,
					x => x.Name)
				.ToList();

			Contacts = await _reader.Query<OrganizationContact>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == Id)
				.OrderByDescending(x => x.IsPrimary)
				.ThenBy(x => x.Name)
				.ToListAsync();

			Products = await (
				from e in _reader.Query<ProductEnablement>().AsNoTracking()
				join p in _reader.Query<Product>().AsNoTracking() on e.ProductId equals p.Id
				where !e.IsDeleted && !p.IsDeleted && e.OrganizationId == Id
				orderby p.Name
				select new ProductRow { Name = p.Name, Code = p.Code }).ToListAsync();

			if (CanViewMembers)
			{
				var members = await (
					from m in _reader.Query<Membership>().AsNoTracking()
					join u in _reader.Query<User>().AsNoTracking() on m.UserId equals u.Id
					where !m.IsDeleted && !u.IsDeleted && m.OrganizationId == Id
					orderby u.Email
					select new { u.Id, Email = u.Email ?? u.UserName ?? "", u.DisplayName }).ToListAsync();

				var userIds = members.Select(x => x.Id).ToList();
				var roleRows = await (
					from a in _reader.Query<UserRoleAssignment>().AsNoTracking()
					join r in _reader.Query<Role>().AsNoTracking() on a.RoleId equals r.Id
					where !a.IsDeleted && !r.IsDeleted && a.OrganizationId == Id && userIds.Contains(a.UserId)
					select new { a.UserId, r.Name }).ToListAsync();

				var rolesByUser = roleRows
					.GroupBy(x => x.UserId)
					.ToDictionary(g => g.Key, g => g.Select(x => x.Name).Distinct().OrderBy(x => x).ToList());

				Members = members.Select(x => new MemberRow
				{
					Email = x.Email,
					DisplayName = x.DisplayName,
					Roles = rolesByUser.TryGetValue(x.Id, out var roles) ? roles : new List<string>()
				}).ToList();
			}

			if (BranchEditId is Guid bid)
			{
				var b = Branches.FirstOrDefault(x => x.Id == bid);
				if (b is not null)
				{
					BranchName = b.Name;
					BranchCode = b.Code;
					BranchLegalName = b.LegalName;
					BranchTradeName = b.TradeName;
					BranchTaxId = b.TaxId;
					BranchSegment = b.Segment;
					BranchDescription = b.Description;
					BranchPostalCode = b.PostalCode;
					BranchStreet = b.Street;
					BranchNumber = b.Number;
					BranchComplement = b.Complement;
					BranchCity = b.City;
					BranchState = b.State;
				}
			}

			if (ContactEditId is Guid cid)
			{
				var c = Contacts.FirstOrDefault(x => x.Id == cid);
				if (c is not null)
				{
					ContactName = c.Name;
					ContactEmail = c.Email;
					ContactPhone = c.Phone;
					ContactTitle = c.Title;
					ContactIsPrimary = c.IsPrimary;
				}
			}
		}

		private static string? EmptyToNull(string? value) =>
			string.IsNullOrWhiteSpace(value) ? null : value.Trim();

		private void ClearBranchForm()
		{
			BranchName = string.Empty;
			BranchCode = string.Empty;
			BranchLegalName = null;
			BranchTradeName = null;
			BranchTaxId = null;
			BranchSegment = null;
			BranchDescription = null;
			BranchPostalCode = null;
			BranchStreet = null;
			BranchNumber = null;
			BranchComplement = null;
			BranchCity = null;
			BranchState = null;
		}
	}
}
