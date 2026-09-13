using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Memberships.Commands;
using SSO.Core.Application.Identity.UserRoleAssignments.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Users
{
	public sealed class DetailsModel : AdminHubPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;
		private readonly IdentityDbContext _db;
		private readonly IUserSessionService _sessions;

		public DetailsModel(
			IAdminPortalContextService portal,
			IIdentityDbContextReader reader,
			IMediator mediator,
			IdentityDbContext db,
			IUserSessionService sessions) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
			_db = db;
			_sessions = sessions;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public string Tab { get; set; } = "dados";

		public string ActiveTab => string.IsNullOrWhiteSpace(Tab) ? "dados" : Tab.ToLowerInvariant();

		public User? Account { get; set; }
		public List<MembershipRow> Memberships { get; set; } = new();
		public List<UserRoleAssignment> Assignments { get; set; } = new();
		public List<UserClaimAssignment> UserClaims { get; set; } = new();
		public IReadOnlyList<UserSession> Sessions { get; set; } = Array.Empty<UserSession>();
		public List<Role> Roles { get; set; } = new();
		public List<Product> Products { get; set; } = new();
		public List<Organization> Organizations { get; set; } = new();
		public List<ClaimDefinition> Definitions { get; set; } = new();

		public bool CanRevokeSessions => Portal.HasPermission(SsoAdminPermissions.SessionsRevoke);

		[BindProperty]
		public Guid RoleId { get; set; }

		[BindProperty]
		public Guid ProductId { get; set; }

		[BindProperty]
		public Guid? AssignmentOrganizationId { get; set; }

		[BindProperty]
		public Guid ClaimDefinitionId { get; set; }

		[BindProperty]
		public string ClaimValue { get; set; } = string.Empty;

		[BindProperty]
		public Guid ClaimProductId { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Account is null ? NotFound() : Page();
		}

		public async Task<IActionResult> OnPostRemoveMembershipAsync(Guid membershipId)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "memberships";
			var cmd = AdminWrap.FromAnonymous<DeleteMembershipCommand>(new { id = membershipId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Membership removida.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAddAssignmentAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "assignments";
			var cmd = AdminWrap.FromAnonymous<PostUserRoleAssignmentCommand>(new
			{
				userId = Id,
				roleId = RoleId,
				organizationId = AssignmentOrganizationId,
				branchId = (Guid?)null,
				productId = ProductId,
				inheritable = false
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Atribuição criada.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostRemoveAssignmentAsync(Guid assignmentId)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "assignments";
			var cmd = AdminWrap.FromAnonymous<DeleteUserRoleAssignmentCommand>(new { id = assignmentId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Atribuição removida.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAddClaimAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "claims";
			var definition = await _db.ClaimDefinitions.AsNoTracking().FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == ClaimDefinitionId);
			if (definition is null)
			{
				Error = "Definição de claim não encontrada.";
			}
			else if (!ClaimValueTypes.TryValidate(definition.ValueType, ClaimValue, out var typeError))
			{
				Error = typeError;
			}
			else
			{
				try
				{
					_db.UserClaimAssignments.Add(UserClaimAssignment.Create(Id, ClaimDefinitionId, ClaimValue, ClaimProductId, null, null, false));
					await _db.SaveChangesAsync();
					Message = "Claim atribuído.";
					ClaimValue = string.Empty;
				}
				catch (Exception ex)
				{
					Error = ex.Message;
				}
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostRemoveClaimAsync(Guid assignmentId)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "claims";
			var entity = await _db.UserClaimAssignments.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == assignmentId && x.UserId == Id);
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

		public async Task<IActionResult> OnPostRevokeSessionAsync(Guid sessionId)
		{
			if (!CanRevokeSessions)
			{
				return Forbid();
			}

			Tab = "sessions";
			var revoked = await _sessions.RevokeSessionAsync(sessionId, "admin.portal.revoke");
			Message = revoked ? "Sessão revogada." : null;
			Error = revoked ? null : "Sessão não encontrada.";
			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Account = await _reader.Query<User>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id && !x.IsDeleted);
			if (Account is null)
			{
				return;
			}

			Roles = await _reader.Query<Role>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Products = await _reader.Query<Product>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Organizations = await _reader.Query<Organization>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Definitions = await _reader.Query<ClaimDefinition>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			Memberships = await (
				from m in _reader.Query<Membership>().AsNoTracking()
				join o in _reader.Query<Organization>().AsNoTracking() on m.OrganizationId equals o.Id
				where !m.IsDeleted && !o.IsDeleted && m.UserId == Id
				orderby o.Name
				select new MembershipRow { Id = m.Id, OrganizationId = o.Id, OrganizationName = o.Name }).ToListAsync();
			Assignments = await _reader.Query<UserRoleAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.UserId == Id)
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();
			UserClaims = await _reader.Query<UserClaimAssignment>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.UserId == Id)
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();
			if (CanRevokeSessions)
			{
				Sessions = await _sessions.ListForUserAsync(Id, includeRevoked: true);
			}
		}

		public sealed class MembershipRow
		{
			public Guid Id { get; set; }
			public Guid OrganizationId { get; set; }
			public string OrganizationName { get; set; } = string.Empty;
		}
	}
}
