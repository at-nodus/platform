using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.RolePermissions.Commands;
using SSO.Core.Application.Identity.Roles.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Roles
{
	public sealed class DetailsModel : AdminHubPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;
		private readonly IdentityDbContext _db;

		public DetailsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator, IdentityDbContext db) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
			_db = db;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public string Tab { get; set; } = "dados";

		public string ActiveTab => string.IsNullOrWhiteSpace(Tab) ? "dados" : Tab.ToLowerInvariant();

		public Role? Role { get; set; }
		public List<RolePermission> RolePermissions { get; set; } = new();
		public List<Permission> AllPermissions { get; set; } = new();
		public List<Permission> AvailablePermissions { get; set; } = new();
		public List<RoleClaim> RoleClaims { get; set; } = new();
		public List<ClaimDefinition> Definitions { get; set; } = new();

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		[BindProperty]
		public Guid PermissionId { get; set; }

		[BindProperty]
		public Guid ClaimDefinitionId { get; set; }

		[BindProperty]
		public string ClaimValue { get; set; } = string.Empty;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Role is null ? NotFound() : Page();
		}

		public async Task<IActionResult> OnPostUpdateAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "dados";
			var cmd = AdminWrap.FromAnonymous<PutRoleCommand>(new { id = Id, code = Code, name = Name });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Role atualizada.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAddPermissionAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "permissions";
			var cmd = AdminWrap.FromAnonymous<PostRolePermissionCommand>(new { roleId = Id, permissionId = PermissionId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Permissão vinculada à role.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostRemovePermissionAsync(Guid rolePermissionId)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "permissions";
			var cmd = AdminWrap.FromAnonymous<DeleteRolePermissionCommand>(new { id = rolePermissionId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Vínculo removido.");
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
					_db.AuthRoleClaims.Add(RoleClaim.Create(Id, ClaimDefinitionId, ClaimValue));
					await _db.SaveChangesAsync();
					Message = "Claim vinculado à role.";
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

		public async Task<IActionResult> OnPostRemoveClaimAsync(Guid roleClaimId)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "claims";
			var entity = await _db.AuthRoleClaims.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == roleClaimId && x.RoleId == Id);
			if (entity is null)
			{
				Error = "Vínculo não encontrado.";
			}
			else
			{
				entity.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Vínculo removido.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Role = await _reader.Query<Role>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id && !x.IsDeleted);
			if (Role is null)
			{
				return;
			}

			Code = Role.Code;
			Name = Role.Name;
			AllPermissions = await _reader.Query<Permission>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			RolePermissions = await _reader.Query<RolePermission>().AsNoTracking().Where(x => !x.IsDeleted && x.RoleId == Id).ToListAsync();
			var linked = RolePermissions.Select(x => x.PermissionId).ToHashSet();
			AvailablePermissions = AllPermissions.Where(x => !linked.Contains(x.Id)).ToList();
			Definitions = await _reader.Query<ClaimDefinition>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Code).ToListAsync();
			RoleClaims = await _reader.Query<RoleClaim>().AsNoTracking().Where(x => !x.IsDeleted && x.RoleId == Id).ToListAsync();
		}
	}
}
