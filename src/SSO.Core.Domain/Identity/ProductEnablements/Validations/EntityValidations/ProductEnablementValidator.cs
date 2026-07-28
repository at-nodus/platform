using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;

namespace SSO.Core.Domain.Identity.ProductEnablements.Validations.EntityValidations
{
	public sealed class ProductEnablementValidator : EntityValidator<ProductEnablement>
	{
		public ProductEnablementValidator()
		{
			RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("'{PropertyName}' is required!");
			RuleFor(x => x.ProductId).NotEmpty().WithMessage("'{PropertyName}' is required!");
		}
	}
}
