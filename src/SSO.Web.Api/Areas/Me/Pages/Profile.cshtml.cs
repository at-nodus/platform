using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.OrganizationInvites.Commands;
using SSO.Core.Application.Identity.Users.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Identity.Roles.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity;

namespace SSO.Web.Api.Areas.Me.Pages
{
	public sealed class ProfileModel : MePageModel
	{
		private readonly UserManager<User> _userManager;
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public ProfileModel(
			IAdminPortalContextService portal,
			UserManager<User> userManager,
			IIdentityDbContextReader reader,
			IMediator mediator) : base(portal)
		{
			_userManager = userManager;
			_reader = reader;
			_mediator = mediator;
		}

		public User? CurrentUser { get; set; }
		public List<OrgMembershipRow> Organizations { get; set; } = new();
		public List<AccessRow> Accesses { get; set; } = new();
		public List<InviteRow> Invites { get; set; } = new();

		[BindProperty]
		public string? DisplayName { get; set; }

		[BindProperty]
		public string? PhoneNumber { get; set; }

		public sealed class OrgMembershipRow
		{
			public Guid OrganizationId { get; set; }
			public string Name { get; set; } = string.Empty;
			public string Code { get; set; } = string.Empty;
		}

		public sealed class AccessRow
		{
			public string ProductName { get; set; } = string.Empty;
			public string? OrganizationName { get; set; }
			public string? BranchName { get; set; }
			public string RoleName { get; set; } = string.Empty;
		}

		public sealed class InviteRow
		{
			public Guid Id { get; set; }
			public string OrganizationName { get; set; } = string.Empty;
			public DateTime CreatedAt { get; set; }
			public string Status { get; set; } = string.Empty;
			public string StatusLabel { get; set; } = string.Empty;
			public bool CanRespond { get; set; }
		}

		public async Task<IActionResult> OnGetAsync()
		{
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostUpdateProfileAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			var result = await _mediator.Send(new PatchMeUserCommand
			{
				UserId = user.Id,
				DisplayName = DisplayName,
				PhoneNumber = PhoneNumber
			});

			if (result.Succeeded)
			{
				Message = "Dados pessoais atualizados.";
			}
			else
			{
				Error = result.Error ?? "Não foi possível atualizar.";
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAcceptInviteAsync(Guid inviteId)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			var result = await _mediator.Send(new PatchAcceptOrganizationInviteByIdCommand
			{
				InviteId = inviteId,
				AcceptingUserId = user.Id,
				AcceptingUserEmail = user.Email ?? string.Empty
			});

			if (result.Succeeded)
			{
				Message = "Convite aceito.";
			}
			else
			{
				Error = result.Error ?? "Falha ao aceitar convite.";
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeclineInviteAsync(Guid inviteId)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null)
			{
				return Challenge();
			}

			var result = await _mediator.Send(new PatchDeclineOrganizationInviteByIdCommand
			{
				InviteId = inviteId,
				ActingUserId = user.Id,
				ActingUserEmail = user.Email ?? string.Empty
			});

			if (result.Succeeded)
			{
				Message = "Convite recusado.";
			}
			else
			{
				Error = result.Error ?? "Falha ao recusar convite.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			var user = await _userManager.GetUserAsync(User);
			CurrentUser = user;
			if (user is null)
			{
				return;
			}

			DisplayName = user.DisplayName;
			PhoneNumber = user.PhoneNumber;

			Organizations = await (
				from m in _reader.Query<Membership>().AsNoTracking()
				join o in _reader.Query<Organization>().AsNoTracking() on m.OrganizationId equals o.Id
				where !m.IsDeleted && !o.IsDeleted && m.UserId == user.Id
				orderby o.Name
				select new OrgMembershipRow
				{
					OrganizationId = o.Id,
					Name = o.Name,
					Code = o.Code
				}).ToListAsync();

			Accesses = await (
				from a in _reader.Query<UserRoleAssignment>().AsNoTracking()
				join r in _reader.Query<Role>().AsNoTracking() on a.RoleId equals r.Id
				join p in _reader.Query<Product>().AsNoTracking() on a.ProductId equals p.Id
				join o in _reader.Query<Organization>().AsNoTracking() on a.OrganizationId equals o.Id into og
				from o in og.DefaultIfEmpty()
				join b in _reader.Query<SSO.Core.Domain.Identity.Branches.Entity.Branch>().AsNoTracking() on a.BranchId equals b.Id into bg
				from b in bg.DefaultIfEmpty()
				where !a.IsDeleted && a.UserId == user.Id
				orderby p.Name, r.Name
				select new AccessRow
				{
					ProductName = p.Name,
					OrganizationName = o != null ? o.Name : null,
					BranchName = b != null ? b.Name : null,
					RoleName = r.Name
				}).ToListAsync();

			var email = user.Email ?? string.Empty;
			var invites = await _reader.Query<OrganizationInvite>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.Email == email)
				.OrderByDescending(x => x.CreatedAt)
				.ToListAsync();

			var orgIds = invites.Select(x => x.OrganizationId).Distinct().ToList();
			var orgNames = await _reader.Query<Organization>().AsNoTracking()
				.Where(x => orgIds.Contains(x.Id))
				.ToDictionaryAsync(x => x.Id, x => x.Name);

			Invites = invites.Select(x =>
			{
				var status = x.Status;
				if (status == OrganizationInviteStatuses.Pending && x.ExpiresAt <= DateTime.UtcNow)
				{
					status = OrganizationInviteStatuses.Expired;
				}

				return new InviteRow
				{
					Id = x.Id,
					OrganizationName = orgNames.TryGetValue(x.OrganizationId, out var n) ? n : x.OrganizationId.ToString(),
					CreatedAt = x.CreatedAt,
					Status = status,
					StatusLabel = status switch
					{
						OrganizationInviteStatuses.Pending => "Pendente",
						OrganizationInviteStatuses.Accepted => "Aceito",
						OrganizationInviteStatuses.Declined => "Recusado",
						OrganizationInviteStatuses.Cancelled => "Cancelado",
						OrganizationInviteStatuses.Expired => "Expirado",
						_ => status
					},
					CanRespond = status == OrganizationInviteStatuses.Pending && x.ExpiresAt > DateTime.UtcNow
				};
			}).ToList();
		}
	}
}
