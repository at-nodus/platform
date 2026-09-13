using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.UserRoleAssignments.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Organizations
{
	public sealed class AssignmentsModel : AdminHubPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public AssignmentsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		public List<UserRoleAssignment> Items { get; set; } = new();
		public List<Role> Roles { get; set; } = new();
		public List<Product> Products { get; set; } = new();
		public List<Branch> Branches { get; set; } = new();
		public List<MemberOption> Members { get; set; } = new();

		[BindProperty]
		public Guid UserId { get; set; }

		[BindProperty]
		public Guid RoleId { get; set; }

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

			var cmd = AdminWrap.FromAnonymous<PostUserRoleAssignmentCommand>(new
			{
				userId = UserId,
				roleId = RoleId,
				organizationId = Id,
				branchId = BranchId,
				productId = ProductId,
				inheritable = Inheritable
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Atribuição criada.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid assignmentId)
		{
			if (!CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteUserRoleAssignmentCommand>(new { id = assignmentId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Atribuição removida.");
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

			Members = await (
				from m in _reader.Query<Membership>().AsNoTracking()
				join u in _reader.Query<User>().AsNoTracking() on m.UserId equals u.Id
				where !m.IsDeleted && !u.IsDeleted && m.OrganizationId == Id
				orderby u.Email
				select new MemberOption { Id = u.Id, Label = u.Email ?? u.UserName ?? u.Id.ToString() }).ToListAsync();

			Items = await _reader.Query<UserRoleAssignment>().AsNoTracking()
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
