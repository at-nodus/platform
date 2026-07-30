using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using FluentValidation;
using SSO.Core.Domain.Identity.Users.Entity;

namespace SSO.Core.Domain.Identity.Users.Validations.EntityValidations
{
	public sealed class UserValidator : EntityValidator<User>
	{
		public UserValidator()
		{
			RuleFor(x => x.Email).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.Email).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.Email).EmailAddress().WithMessage("'{PropertyName}' is not a valid email!");
			RuleFor(x => x.Email).MaximumLength(256).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");

			RuleFor(x => x.UserName).NotNull().WithMessage("'{PropertyName}' cannot be null!");
			RuleFor(x => x.UserName).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
			RuleFor(x => x.UserName).MaximumLength(256).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!");

			RuleFor(x => x.DisplayName)
				.MaximumLength(200).WithMessage("'{PropertyName}' must have a maximum of '{MaxLength}' caracters!")
				.When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

			// Password is NotMapped: required only when provided (create). Profile update leaves it null.
			When(x => x.Password != null, () =>
			{
				RuleFor(x => x.Password).NotEmpty().WithMessage("'{PropertyName}' cannot be empty!");
				RuleFor(x => x.Password).MinimumLength(8).WithMessage("'{PropertyName}' must have at least '{MinLength}' caracters!");
			});
		}
	}
}
