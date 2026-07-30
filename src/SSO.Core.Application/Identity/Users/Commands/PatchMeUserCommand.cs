using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity.Users.Services;

namespace SSO.Core.Application.Identity.Users.Commands
{
	public sealed class PatchMeUserCommand : IRequest<PatchMeUserResult>
	{
		public Guid UserId { get; set; }
		public string? DisplayName { get; set; }
		public string? PhoneNumber { get; set; }
	}

	public sealed class PatchMeUserResult
	{
		public bool Succeeded { get; init; }
		public string? Error { get; init; }
		public User? User { get; init; }
	}

	public sealed class PatchMeUserCommandHandler : IRequestHandler<PatchMeUserCommand, PatchMeUserResult>
	{
		private readonly UserManager<User> _userManager;
		private readonly IMediator _mediator;

		public PatchMeUserCommandHandler(UserManager<User> userManager, IMediator mediator)
		{
			_userManager = userManager;
			_mediator = mediator;
		}

		public async Task<PatchMeUserResult> Handle(PatchMeUserCommand request, CancellationToken cancellationToken)
		{
			var user = await _userManager.FindByIdAsync(request.UserId.ToString());
			if (user is null || user.IsDeleted)
			{
				return new PatchMeUserResult { Succeeded = false, Error = "Usuário não encontrado." };
			}

			// Email is immutable on self-profile (D-00014-5).
			user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
				? null
				: request.DisplayName.Trim();
			user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
				? null
				: request.PhoneNumber.Trim();

			try
			{
				await _mediator.Send(new UpdateUserProfileServiceRequest(user), cancellationToken);
				return new PatchMeUserResult { Succeeded = true, User = user };
			}
			catch (Exception ex)
			{
				return new PatchMeUserResult { Succeeded = false, Error = ex.Message };
			}
		}
	}
}
