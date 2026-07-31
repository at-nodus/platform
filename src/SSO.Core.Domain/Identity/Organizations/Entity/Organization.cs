using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.Organizations.Resources;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using SSO.Core.Domain.Resources;
using System.Collections.Generic;

namespace SSO.Core.Domain.Identity.Organizations.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityOrganization), Priority = 0)]
	public sealed class Organization : IdentityAuditableEntity
	{
		public string Name { get; set; }
		public string Code { get; set; }
		/// <summary>Razão social.</summary>
		public string LegalName { get; set; }
		/// <summary>Nome fantasia.</summary>
		public string TradeName { get; set; }
		/// <summary>CNPJ (raiz / matriz).</summary>
		public string TaxId { get; set; }
		public string Segment { get; set; }
		public string Description { get; set; }
		public string PostalCode { get; set; }
		public string Street { get; set; }
		public string Number { get; set; }
		public string Complement { get; set; }
		public string City { get; set; }
		/// <summary>UF (2 letras).</summary>
		public string State { get; set; }
		/// <summary>Off | InheritFromAncestors (ADR-008 / F00009-D1). Default Off.</summary>
		public string BranchAuthzInheritance { get; set; } = SSO.Shared.Identity.BranchAuthzInheritancePolicies.Off;

		public ICollection<Branch> Branches { get; set; } = new List<Branch>();
		public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
		public ICollection<OrganizationInvite> OrganizationInvites { get; set; } = new List<OrganizationInvite>();
		public ICollection<ProductEnablement> ProductEnablements { get; set; } = new List<ProductEnablement>();
		public ICollection<OrganizationContact> OrganizationContacts { get; set; } = new List<OrganizationContact>();
		public ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
		public ICollection<UserClaimAssignment> UserClaimAssignments { get; set; } = new List<UserClaimAssignment>();
		public ICollection<ExternalIdentityProvider> ExternalIdentityProviders { get; set; } = new List<ExternalIdentityProvider>();
		public ICollection<LdapGroupRoleMap> LdapGroupRoleMaps { get; set; } = new List<LdapGroupRoleMap>();
		public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

		public Organization()
		{
		}
	}
}
