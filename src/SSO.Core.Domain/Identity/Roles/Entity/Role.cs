using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.Roles.Resources;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Resources;
using System.Collections.Generic;

namespace SSO.Core.Domain.Identity.Roles.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityRole), Priority = 0)]
	public sealed class Role : IdentityAuditableEntity
	{
		public string Code { get; set; }
		public string Name { get; set; }

		public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
		public ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
		public ICollection<RoleClaim> RoleClaims { get; set; } = new List<RoleClaim>();
		public ICollection<LdapGroupRoleMap> LdapGroupRoleMaps { get; set; } = new List<LdapGroupRoleMap>();

		public Role()
		{
		}
	}
}
