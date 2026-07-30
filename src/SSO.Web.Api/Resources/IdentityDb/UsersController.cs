using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ModelWrapper;
using SSO.Core.Application.Identity.Users.Commands;
using SSO.Core.Application.Identity.Users.Queries;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[ApiController]
	[Produces("application/json")]
	[Route("api/identity/users")]
	public sealed class UsersController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly UserManager<User> _userManager;

		public UsersController(IMediator mediator, UserManager<User> userManager)
		{
			_mediator = mediator;
			_userManager = userManager;
		}

		[HttpGet("me")]
		[Authorize]
		public async Task<IActionResult> GetMe(CancellationToken cancellationToken = default)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null || user.IsDeleted)
			{
				return Unauthorized();
			}

			return Ok(new
			{
				id = user.Id,
				email = user.Email,
				userName = user.UserName,
				displayName = user.DisplayName,
				phoneNumber = user.PhoneNumber,
				emailConfirmed = user.EmailConfirmed
			});
		}

		[HttpPatch("me")]
		[Authorize]
		public async Task<IActionResult> PatchMe([FromBody] PatchMeBody body, CancellationToken cancellationToken = default)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user is null || user.IsDeleted)
			{
				return Unauthorized();
			}

			var result = await _mediator.Send(new PatchMeUserCommand
			{
				UserId = user.Id,
				DisplayName = body?.DisplayName,
				PhoneNumber = body?.PhoneNumber
			}, cancellationToken);

			if (!result.Succeeded)
			{
				return BadRequest(new { error = result.Error });
			}

			return Ok(new
			{
				id = result.User!.Id,
				email = result.User.Email,
				displayName = result.User.DisplayName,
				phoneNumber = result.User.PhoneNumber
			});
		}

		[HttpGet]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<ActionResult<GetUsersByFilterQueryResponse>> GetByFilter(
			GetUsersByFilterQuery request,
			CancellationToken cancellationToken = default)
			=> Wrap(await _mediator.Send(request, cancellationToken));

		[HttpGet("{id:Guid}")]
		[RequiresPermission(SsoAdminPermissions.Platform, SsoAdminPermissions.Org)]
		public async Task<ActionResult<GetUserByIdQueryResponse>> Get(GetUserByIdQuery request, CancellationToken cancellationToken = default)
			=> Wrap(await _mediator.Send(request, cancellationToken));

		[HttpPost]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<ActionResult<PostUserCommandResponse>> Post(PostUserCommand request, CancellationToken cancellationToken = default)
			=> Wrap(await _mediator.Send(request, cancellationToken));

		private ActionResult Wrap(WrapResponse response)
		{
			return new ObjectResult(response) { StatusCode = response.StatusCode };
		}

		public sealed class PatchMeBody
		{
			public string? DisplayName { get; set; }
			public string? PhoneNumber { get; set; }
		}
	}
}
