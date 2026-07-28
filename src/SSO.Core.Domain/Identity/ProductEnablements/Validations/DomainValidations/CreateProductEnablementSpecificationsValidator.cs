using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Specifications;

namespace SSO.Core.Domain.Identity.ProductEnablements.Validations.DomainValidations
{
	public sealed class CreateProductEnablementSpecificationsValidator : DomainValidator<ProductEnablement>
	{
		public CreateProductEnablementSpecificationsValidator(ProductEnablementOrganizationProductAlreadyExistsSpecification spec)
		{
			Add(nameof(spec), new DomainRule<ProductEnablement>(spec.Not(), spec.ToString()));
		}
	}
}
