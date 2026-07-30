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
	public sealed class DeleteOrganizationContactServiceRequest : DomainServiceRequest<OrganizationContact>
	{
		public DeleteOrganizationContactServiceRequest(OrganizationContact payload) : base(payload) { }
	}

	public sealed class DeleteOrganizationContactServiceRequestHandler
		: DomainServiceRequestHandler<OrganizationContact, DeleteOrganizationContactServiceRequest>
	{
		private IIdentityDbContextWriter Writer { get; set; }
		public DeleteOrganizationContactServiceRequestHandler(
			IIdentityDbContextWriter writer,
			IStringLocalizer<OrganizationContact> localizer,
			OrganizationContactValidator entityValidator,
			DeleteOrganizationContactSpecificationsValidator domainValidator)
			: base(localizer, entityValidator, domainValidator)
		{
			Writer = writer;
		}

		override public async Task<OrganizationContact> Handle(DeleteOrganizationContactServiceRequest request, CancellationToken cancellationToken)
		{
			ValidateEntity(request.Payload);
			ValidateDomain(request.Payload);
			request.Payload.MarkDeleted();
			return request.Payload;
		}
	}
}
