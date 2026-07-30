using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.OrganizationContacts.Resources;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.OrganizationContacts.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityOrganizationContact), Priority = 0)]
	public sealed class OrganizationContact : IdentityAuditableEntity
	{
		public Guid OrganizationId { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Title { get; set; }
		public bool IsPrimary { get; set; }

		public Organization Organization { get; set; }

		public OrganizationContact()
		{
		}
	}
}
