using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.MenuItems.Specifications;

namespace SSO.Core.Domain.Identity.MenuItems.Validations.DomainValidations
{
	public sealed class CreateMenuItemSpecificationsValidator : DomainValidator<MenuItem>
	{
		public CreateMenuItemSpecificationsValidator(MenuItemPermissionCodeDoesNotExistSpecification spec)
		{
			Add(nameof(spec), new DomainRule<MenuItem>(spec.Not(), spec.ToString()));
		}
	}
}
