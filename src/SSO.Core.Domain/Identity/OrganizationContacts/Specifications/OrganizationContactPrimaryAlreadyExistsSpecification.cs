using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.OrganizationContacts.Specifications
{
	public sealed class OrganizationContactPrimaryAlreadyExistsSpecification : DomainSpecification<OrganizationContact>
	{
		private IIdentityDbContextReader Reader { get; set; }

		public OrganizationContactPrimaryAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A primary contact already exists for this organization!";
		}

		override public Expression<Func<OrganizationContact, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(OrganizationContact entity)
		{
			if (!entity.IsPrimary)
			{
				return false;
			}

			return Reader.Query<OrganizationContact>()
				.Any(x => !x.IsDeleted && x.IsPrimary && x.OrganizationId == entity.OrganizationId && x.Id != entity.Id);
		}
	}
}
