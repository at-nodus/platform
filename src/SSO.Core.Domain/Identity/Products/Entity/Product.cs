using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.Products.Resources;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Resources;
using System.Collections.Generic;

namespace SSO.Core.Domain.Identity.Products.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityProduct), Priority = 0)]
	public sealed class Product : IdentityAuditableEntity
	{
		public string Name { get; set; }
		public string Code { get; set; }

		public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
		public ICollection<ClaimDefinition> ClaimDefinitions { get; set; } = new List<ClaimDefinition>();
		public ICollection<ClientProductBinding> ClientProductBindings { get; set; } = new List<ClientProductBinding>();
		public ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
		public ICollection<UserClaimAssignment> UserClaimAssignments { get; set; } = new List<UserClaimAssignment>();
		public ICollection<LdapGroupRoleMap> LdapGroupRoleMaps { get; set; } = new List<LdapGroupRoleMap>();

		public Product()
		{
		}
	}
}
