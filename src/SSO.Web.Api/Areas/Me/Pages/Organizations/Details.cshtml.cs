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
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
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

		[BindProperty]
		public string OrgName { get; set; } = string.Empty;

		[BindProperty]
		public string OrgCode { get; set; } = string.Empty;

		[BindProperty]
		public string BranchName { get; set; } = string.Empty;

		[BindProperty]
		public string BranchCode { get; set; } = string.Empty;

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
				branchAuthzInheritance = Organization?.BranchAuthzInheritance ?? BranchAuthzInheritancePolicies.Off
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Empresa atualizada.");
			await LoadAsync();
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
				code = BranchCode
			});
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Filial criada."))
			{
				BranchName = string.Empty;
				BranchCode = string.Empty;
			}

			await LoadAsync();
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
				parentBranchId = existing.ParentBranchId
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Filial atualizada.");
			BranchEditId = null;
			BranchName = string.Empty;
			BranchCode = string.Empty;
			await LoadAsync();
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

			Branches = await _reader.Query<Branch>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == Id)
				.OrderBy(x => x.Name)
				.ToListAsync();

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
				Members = await (
					from m in _reader.Query<Membership>().AsNoTracking()
					join u in _reader.Query<User>().AsNoTracking() on m.UserId equals u.Id
					where !m.IsDeleted && !u.IsDeleted && m.OrganizationId == Id
					orderby u.Email
					select new MemberRow { Email = u.Email ?? u.UserName ?? "", DisplayName = u.DisplayName }).ToListAsync();
			}

			if (BranchEditId is Guid bid)
			{
				var b = Branches.FirstOrDefault(x => x.Id == bid);
				if (b is not null)
				{
					BranchName = b.Name;
					BranchCode = b.Code;
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
	}
}
