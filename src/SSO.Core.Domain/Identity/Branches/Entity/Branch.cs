using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Branches.Resources;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using SSO.Core.Domain.Resources;
using System;
using System.Collections.Generic;

namespace SSO.Core.Domain.Identity.Branches.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityBranch), Priority = 0)]
	public sealed class Branch : IdentityAuditableEntity
	{
		public Guid OrganizationId { get; set; }
		public Guid? ParentBranchId { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }

		public Organization Organization { get; set; }
		public Branch ParentBranch { get; set; }
		public ICollection<Branch> ChildBranches { get; set; } = new List<Branch>();
		public ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
		public ICollection<UserClaimAssignment> UserClaimAssignments { get; set; } = new List<UserClaimAssignment>();
		public ICollection<LdapGroupRoleMap> LdapGroupRoleMaps { get; set; } = new List<LdapGroupRoleMap>();
		public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

		public Branch()
		{
		}
	}
}
