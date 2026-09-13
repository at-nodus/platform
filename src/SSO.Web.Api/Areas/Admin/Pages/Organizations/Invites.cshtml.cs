using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.OrganizationInvites.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Organizations
{
	public sealed class InvitesModel : AdminHubPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public InvitesModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		[BindProperty(SupportsGet = true)]
		public Guid Id { get; set; }

		public List<OrganizationInvite> Items { get; set; } = new();

		[BindProperty]
		public string Email { get; set; } = string.Empty;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanAccessOrganization(Id))
			{
				return Forbid();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostSendAsync()
		{
			if (!CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PostOrganizationInviteCommand>(new { organizationId = Id, email = Email });
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, $"Convite enviado para {Email.Trim().ToLowerInvariant()}."))
			{
				Email = string.Empty;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostCancelAsync(Guid inviteId)
		{
			if (!CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PatchCancelOrganizationInviteCommand>(new { id = inviteId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Convite cancelado.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostResendAsync(Guid inviteId)
		{
			if (!CanAccessOrganization(Id))
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PatchResendOrganizationInviteCommand>(new { id = inviteId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Convite reenviado.");
			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Items = await _reader.Query<OrganizationInvite>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.OrganizationId == Id)
				.OrderByDescending(x => x.CreatedAt)
				.Take(100)
				.ToListAsync();
		}
	}
}
