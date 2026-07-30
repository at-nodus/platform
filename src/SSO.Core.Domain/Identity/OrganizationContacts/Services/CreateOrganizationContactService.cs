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
	public sealed class CreateOrganizationContactServiceRequest : DomainServiceRequest<OrganizationContact>
	{
		public CreateOrganizationContactServiceRequest(OrganizationContact payload) : base(payload) { }
	}

	public sealed class CreateOrganizationContactServiceRequestHandler
		: DomainServiceRequestHandler<OrganizationContact, CreateOrganizationContactServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public CreateOrganizationContactServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<OrganizationContact> localizer,
			OrganizationContactValidator entityValidator,
			CreateOrganizationContactSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<OrganizationContact> Handle(CreateOrganizationContactServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			await Writer.AddAsync(request.Payload);
			return request.Payload;
		}
	}
}
