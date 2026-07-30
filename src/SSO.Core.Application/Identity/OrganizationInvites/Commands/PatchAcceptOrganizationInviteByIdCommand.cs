using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationInvites;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Services;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Core.Application.Identity.OrganizationInvites.Commands
{
	/// <summary>Accept invite by id for the authenticated user (profile UI). Email must match.</summary>
	public sealed class PatchAcceptOrganizationInviteByIdCommand : IRequest<PatchOrganizationInviteResult>
	{
		public Guid InviteId { get; set; }
		public Guid AcceptingUserId { get; set; }
		public string AcceptingUserEmail { get; set; } = string.Empty;
	}

	public sealed class PatchDeclineOrganizationInviteByIdCommand : IRequest<PatchOrganizationInviteResult>
	{
		public Guid InviteId { get; set; }
		public Guid ActingUserId { get; set; }
		public string ActingUserEmail { get; set; } = string.Empty;
	}

	public sealed class PatchAcceptOrganizationInviteByIdCommandHandler
		: IRequestHandler<PatchAcceptOrganizationInviteByIdCommand, PatchOrganizationInviteResult>
	{
		private readonly IIdentityDbContextWriter _writer;
		private readonly IMediator _mediator;

		public PatchAcceptOrganizationInviteByIdCommandHandler(IIdentityDbContextWriter writer, IMediator mediator)
		{
			_writer = writer;
			_mediator = mediator;
		}

		public async Task<PatchOrganizationInviteResult> Handle(
			PatchAcceptOrganizationInviteByIdCommand request,
			CancellationToken cancellationToken)
		{
			var invite = await _writer.Query<OrganizationInvite>()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.InviteId, cancellationToken);

			if (invite is null)
			{
				return Fail("Convite inválido.");
			}

			if (!string.Equals(invite.Email, request.AcceptingUserEmail, StringComparison.OrdinalIgnoreCase))
			{
				return Fail("Este convite não pertence ao seu e-mail.");
			}

			try
			{
				var serviceRequest = new AcceptOrganizationInviteServiceRequest(invite, request.AcceptingUserId);
				await _mediator.Send(serviceRequest, cancellationToken);
				await _writer.CommitAsync(cancellationToken);

				return new PatchOrganizationInviteResult
				{
					Succeeded = true,
					OrganizationId = invite.OrganizationId,
					MembershipId = serviceRequest.MembershipId
				};
			}
			catch (Exception ex)
			{
				if (invite.Status == OrganizationInviteStatuses.Expired)
				{
					await _writer.CommitAsync(cancellationToken);
				}

				return Fail(ex.Message);
			}
		}

		private static PatchOrganizationInviteResult Fail(string error)
			=> new() { Succeeded = false, Error = error };
	}

	public sealed class PatchDeclineOrganizationInviteByIdCommandHandler
		: IRequestHandler<PatchDeclineOrganizationInviteByIdCommand, PatchOrganizationInviteResult>
	{
		private readonly IIdentityDbContextWriter _writer;
		private readonly IMediator _mediator;

		public PatchDeclineOrganizationInviteByIdCommandHandler(IIdentityDbContextWriter writer, IMediator mediator)
		{
			_writer = writer;
			_mediator = mediator;
		}

		public async Task<PatchOrganizationInviteResult> Handle(
			PatchDeclineOrganizationInviteByIdCommand request,
			CancellationToken cancellationToken)
		{
			var invite = await _writer.Query<OrganizationInvite>()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.InviteId, cancellationToken);

			if (invite is null)
			{
				return Fail("Convite inválido.");
			}

			if (!string.Equals(invite.Email, request.ActingUserEmail, StringComparison.OrdinalIgnoreCase))
			{
				return Fail("Este convite não pertence ao seu e-mail.");
			}

			try
			{
				await _mediator.Send(
					new DeclineOrganizationInviteServiceRequest(invite, request.ActingUserId),
					cancellationToken);
				await _writer.CommitAsync(cancellationToken);
				return new PatchOrganizationInviteResult { Succeeded = true, OrganizationId = invite.OrganizationId };
			}
			catch (Exception ex)
			{
				return Fail(ex.Message);
			}
		}

		private static PatchOrganizationInviteResult Fail(string error)
			=> new() { Succeeded = false, Error = error };
	}
}
