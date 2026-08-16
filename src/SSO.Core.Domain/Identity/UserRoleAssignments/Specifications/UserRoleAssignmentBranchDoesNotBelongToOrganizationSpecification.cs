using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.UserRoleAssignments.Specifications
{
	public sealed class UserRoleAssignmentBranchDoesNotBelongToOrganizationSpecification : DomainSpecification<UserRoleAssignment>
	{
		private IIdentityDbContextReader Reader { get; set; }

		public UserRoleAssignmentBranchDoesNotBelongToOrganizationSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "Branch does not belong to the specified organization!";
		}

		override public Expression<Func<UserRoleAssignment, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(UserRoleAssignment entity)
		{
			if (entity.BranchId is null)
			{
				return false;
			}

			if (entity.OrganizationId is null)
			{
				return true;
			}

			var branch = Reader.Query<Branch>()
				.FirstOrDefault(x => !x.IsDeleted && x.Id == entity.BranchId.Value);

			return branch is null || branch.OrganizationId != entity.OrganizationId.Value;
		}
	}
}
