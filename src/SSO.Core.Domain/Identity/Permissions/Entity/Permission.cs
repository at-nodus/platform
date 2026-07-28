using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Permissions.Resources;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Resources;
using System.Collections.Generic;

namespace SSO.Core.Domain.Identity.Permissions.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityPermission), Priority = 0)]
	public sealed class Permission : IdentityAuditableEntity
	{
		public string Code { get; set; }
		public string Name { get; set; }

		public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

		public Permission()
		{
		}
	}
}
