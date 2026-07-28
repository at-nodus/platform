using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.ProductEnablements.Specifications
{
	public class ProductEnablementOrganizationProductAlreadyExistsSpecification : DomainSpecification<ProductEnablement>
	{
		private IIdentityDbContextReader Reader { get; set; }
		public ProductEnablementOrganizationProductAlreadyExistsSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "A product enablement for this organization and product already exists!";
		}

		override public Expression<Func<ProductEnablement, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(ProductEnablement entity)
		{
			return Reader.Query<ProductEnablement>().Any(x =>
				!x.IsDeleted
				&& x.OrganizationId == entity.OrganizationId
				&& x.ProductId == entity.ProductId
				&& x.Id != entity.Id);
		}
	}
}
