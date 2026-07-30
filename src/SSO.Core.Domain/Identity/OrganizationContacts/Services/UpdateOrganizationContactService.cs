using BAYSOFT.Abstractions.Core.Domain.Entities.Services;
using Microsoft.Extensions.Localization;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using SSO.Core.Domain.Identity.OrganizationContacts.Validations.DomainValidations;
using SSO.Core.Domain.Identity.OrganizationContacts.Validations.EntityValidations;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity.OrganizationContacts.Services
{
	public sealed class UpdateOrganizationContactServiceRequest : DomainServiceRequest<OrganizationContact>
	{
		public UpdateOrganizationContactServiceRequest(OrganizationContact payload) : base(payload) { }
	}

	public sealed class UpdateOrganizationContactServiceRequestHandler
		: DomainServiceRequestHandler<OrganizationContact, UpdateOrganizationContactServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public UpdateOrganizationContactServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<OrganizationContact> localizer,
			OrganizationContactValidator entityValidator,
			UpdateOrganizationContactSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<OrganizationContact> Handle(UpdateOrganizationContactServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.TouchUpdated();
			// tracked entity
			return request.Payload;
		}
	}
}
