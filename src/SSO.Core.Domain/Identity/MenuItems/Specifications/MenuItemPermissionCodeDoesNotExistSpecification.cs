using BAYSOFT.Abstractions.Core.Domain.Entities.Specifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.Permissions.Entity;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SSO.Core.Domain.Identity.MenuItems.Specifications
{
	public sealed class MenuItemPermissionCodeDoesNotExistSpecification : DomainSpecification<MenuItem>
	{
		private IIdentityDbContextReader Reader { get; set; }

		public MenuItemPermissionCodeDoesNotExistSpecification(IIdentityDbContextReader reader)
		{
			Reader = reader;
			SpecificationMessage = "Permission code does not exist!";
		}

		override public Expression<Func<MenuItem, bool>> ToExpression()
			=> entity => CheckRule(entity);

		private bool CheckRule(MenuItem entity)
		{
			if (string.IsNullOrWhiteSpace(entity.PermissionCode))
			{
				return false;
			}

			return !Reader.Query<Permission>()
				.Any(x => !x.IsDeleted && x.Code == entity.PermissionCode);
		}
	}
}
