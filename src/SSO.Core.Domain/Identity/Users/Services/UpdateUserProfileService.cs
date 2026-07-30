using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Identity.Users.Validations.DomainValidations;
using SSO.Core.Domain.Identity.Users.Validations.EntityValidations;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.Users.Services
{
	public sealed class UpdateUserProfileServiceRequest : DomainServiceRequest<User>
	{
		public UpdateUserProfileServiceRequest(User payload) : base(payload)
		{
		}
	}

	public sealed class UpdateUserProfileServiceRequestHandler
		: DomainServiceRequestHandler<User, UpdateUserProfileServiceRequest>
	{
		private UserManager<User> UserManager { get; set; }

		public UpdateUserProfileServiceRequestHandler(
			UserManager<User> userManager,
			IStringLocalizer<User> localizer,
			UserValidator entityValidator,
			UpdateUserProfileSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			UserManager = userManager;
		}

		public override async Task<User> Handle(UpdateUserProfileServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();

			var result = await UserManager.UpdateAsync(request.Payload);
			if (!result.Succeeded)
			{
				var message = string.Join(" ", result.Errors.Select(e => e.Description));
				throw new InvalidOperationException(message);
			}

			return request.Payload;
		}
	}
}
