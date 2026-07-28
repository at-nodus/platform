using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Memberships.Resources;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Users.Entity;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.Memberships.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityMembership), Priority = 0)]
	public sealed class Membership : IdentityAuditableEntity
	{
		public Guid UserId { get; set; }
		public Guid OrganizationId { get; set; }

		public User User { get; set; }
		public Organization Organization { get; set; }

		public Membership()
		{
		}
	}
}
