using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.Branches.Entity;

namespace SSO.Core.Domain.Identity.Branches.Validations.EntityValidations
{
	public sealed class BranchValidator : EntityValidator<Branch>
	{
		public BranchValidator()
		{
			RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("'{PropertyName}' is required!");
			RuleFor(x => x.Name).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Name).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Name).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Code).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Code).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Code).MaximumLength(64).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.LegalName).MaximumLength(256).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.TradeName).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.TaxId).MaximumLength(18).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Segment).MaximumLength(64).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Description).MaximumLength(1000).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.PostalCode).MaximumLength(16).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Street).MaximumLength(256).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Number).MaximumLength(32).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.Complement).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.City).MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
			RuleFor(x => x.State).MaximumLength(2).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");
		}
	}
}
