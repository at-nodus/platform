using BAYSOFT.Abstractions.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSO.Core.Domain.Identity.Users.Entity
{
	public sealed class User : IdentityUser<Guid>, IDomainEntityBase
	{
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public DateTime? DeletedAt { get; set; }
		public bool IsDeleted { get; set; }

		/// <summary>Optional display name for profile UI (max 200).</summary>
		public string DisplayName { get; set; }

		[NotMapped]
		public string Password { get; set; }

		public ICollection<Membership> Memberships { get; set; } = new List<Membership>();
		public ICollection<OrganizationInvite> InvitesSent { get; set; } = new List<OrganizationInvite>();
		public ICollection<OrganizationInvite> InvitesAccepted { get; set; } = new List<OrganizationInvite>();
		public ICollection<UserRoleAssignment> UserRoleAssignments { get; set; } = new List<UserRoleAssignment>();
		public ICollection<UserClaimAssignment> UserClaimAssignments { get; set; } = new List<UserClaimAssignment>();
		public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

		public User()
		{
			Id = Guid.NewGuid();
		}

		public void MarkCreated()
		{
			CreatedAt = DateTime.UtcNow;
			IsDeleted = false;
			DeletedAt = null;
		}

		public void TouchUpdated()
		{
			UpdatedAt = DateTime.UtcNow;
		}

		public void MarkDeleted()
		{
			IsDeleted = true;
			DeletedAt = DateTime.UtcNow;
			UpdatedAt = DeletedAt;
		}
	}
}
