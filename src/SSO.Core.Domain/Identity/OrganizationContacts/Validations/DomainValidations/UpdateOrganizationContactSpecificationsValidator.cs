using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using SSO.Core.Domain.Identity.OrganizationContacts.Specifications;

namespace SSO.Core.Domain.Identity.OrganizationContacts.Validations.DomainValidations
{
	public sealed class UpdateOrganizationContactSpecificationsValidator : DomainValidator<OrganizationContact>
	{
		public UpdateOrganizationContactSpecificationsValidator(OrganizationContactPrimaryAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<OrganizationContact>(spec.Not(), spec.ToString()));
		}
	}
}
