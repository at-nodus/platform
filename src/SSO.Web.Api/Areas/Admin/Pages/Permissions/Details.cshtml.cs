using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Permissions.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Permissions
{
	public sealed class DetailsModel : AdminHubPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public DetailsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		public Permission? Permission { get; set; }
		public List<Role> Roles { get; set; } = new();

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Permission is null ? NotFound() : Page();
		}

		public async Task<IActionResult> OnPostUpdateAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PutPermissionCommand>(new { id = Id, code = Code, name = Name });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Permissão atualizada.");
			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Permission = await _reader.Query<Permission>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id && !x.IsDeleted);
			if (Permission is null)
			{
				return;
			}

			Code = Permission.Code;
			Name = Permission.Name;
			Roles = await (
				from rp in _reader.Query<RolePermission>().AsNoTracking()
				join r in _reader.Query<Role>().AsNoTracking() on rp.RoleId equals r.Id
				where !rp.IsDeleted && !r.IsDeleted && rp.PermissionId == Id
				orderby r.Code
				select r).ToListAsync();
		}
	}
}
