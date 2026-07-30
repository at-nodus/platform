using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;

namespace SSO.Core.Domain.Identity.OrganizationContacts.Validations.EntityValidations
{
	public sealed class OrganizationContactValidator : EntityValidator<OrganizationContact>
	{
		public OrganizationContactValidator()
		{
			RuleFor(x => x.OrganizationId).NotEmpty().WithMessage("'{PropertyName}' is required!");

			RuleFor(x => x.Name).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Name).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Name).MaximumLength(200).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");

			RuleFor(x => x.Email)
				.EmailAddress().WithMessage("'{PropertyName}' is not a valid email!")
				.When(x => !string.IsNullOrWhiteSpace(x.Email));
			RuleFor(x => x.Email)
				.MaximumLength(256).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!")
				.When(x => !string.IsNullOrWhiteSpace(x.Email));

			RuleFor(x => x.Phone)
				.MaximumLength(64).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!")
				.When(x => !string.IsNullOrWhiteSpace(x.Phone));

			RuleFor(x => x.Title)
				.MaximumLength(128).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!")
				.When(x => !string.IsNullOrWhiteSpace(x.Title));
		}
	}
}
