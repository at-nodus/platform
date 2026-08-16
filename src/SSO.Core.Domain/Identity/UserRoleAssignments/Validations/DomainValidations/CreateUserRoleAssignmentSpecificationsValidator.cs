using BAYSOFT.Abstractions.Core.Domain.Entities.Validations;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Specifications;

namespace SSO.Core.Domain.Identity.UserRoleAssignments.Validations.DomainValidations
{
	public sealed class CreateUserRoleAssignmentSpecificationsValidator : DomainValidator<UserRoleAssignment>
	{
		public CreateUserRoleAssignmentSpecificationsValidator(UserRoleAssignmentBranchDoesNotBelongToOrganizationSpecification spec)
		{
			Add(nameof(spec), new DomainRule<UserRoleAssignment>(spec.Not(), spec.ToString()));
		}
	}
}
