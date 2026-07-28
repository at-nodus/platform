using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SSO.Core.Domain.Identity.AuthAuditEvents.Entity;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.ExternalIdentityProviders.Entity;
using SSO.Core.Domain.Identity.LdapGroupRoleMaps.Entity;
using SSO.Core.Domain.Identity.Memberships.Entity;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.OrganizationInvites.Entity;
using SSO.Core.Domain.Identity.RevokedSessions.Entity;
using SSO.Core.Domain.Identity.RoleClaims.Entity;
using SSO.Core.Domain.Identity.RolePermissions.Entity;
using SSO.Core.Domain.Identity.UserClaimAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserSessions.Entity;
using SSO.Core.Domain.Identity.WebhookOutbox.Entity;
using SSO.Tests.Helpers.Data.Identity;

namespace SSO.Tests.UnitTests.Infrastructures.Data.Identity
{
	[TestClass]
	public class IdentityForeignKeyModelScenarios
	{
		[TestMethod]
		public void Identity_Model_Should_Declare_Restrict_FKs_For_Guid_Relationships()
		{
			using var context = IdentityDbContextExtensions.GetInMemoryIdentityDbContext(nameof(Identity_Model_Should_Declare_Restrict_FKs_For_Guid_Relationships));
			var model = context.Model;

			AssertHasFk<Membership>(model, nameof(Membership.UserId));
			AssertHasFk<Membership>(model, nameof(Membership.OrganizationId));
			AssertHasFk<Branch>(model, nameof(Branch.OrganizationId));
			AssertHasFk<Branch>(model, nameof(Branch.ParentBranchId));
			AssertHasFk<RolePermission>(model, nameof(RolePermission.RoleId));
			AssertHasFk<RolePermission>(model, nameof(RolePermission.PermissionId));
			AssertHasFk<OrganizationInvite>(model, nameof(OrganizationInvite.OrganizationId));
			AssertHasFk<OrganizationInvite>(model, nameof(OrganizationInvite.InvitedByUserId));
			AssertHasFk<OrganizationInvite>(model, nameof(OrganizationInvite.AcceptedUserId));
			AssertHasFk<UserRoleAssignment>(model, nameof(UserRoleAssignment.UserId));
			AssertHasFk<UserRoleAssignment>(model, nameof(UserRoleAssignment.RoleId));
			AssertHasFk<UserRoleAssignment>(model, nameof(UserRoleAssignment.OrganizationId));
			AssertHasFk<UserRoleAssignment>(model, nameof(UserRoleAssignment.BranchId));
			AssertHasFk<UserRoleAssignment>(model, nameof(UserRoleAssignment.ProductId));
			AssertHasFk<UserClaimAssignment>(model, nameof(UserClaimAssignment.UserId));
			AssertHasFk<UserClaimAssignment>(model, nameof(UserClaimAssignment.ClaimDefinitionId));
			AssertHasFk<UserClaimAssignment>(model, nameof(UserClaimAssignment.OrganizationId));
			AssertHasFk<UserClaimAssignment>(model, nameof(UserClaimAssignment.BranchId));
			AssertHasFk<UserClaimAssignment>(model, nameof(UserClaimAssignment.ProductId));
			AssertHasFk<RoleClaim>(model, nameof(RoleClaim.RoleId));
			AssertHasFk<RoleClaim>(model, nameof(RoleClaim.ClaimDefinitionId));
			AssertHasFk<MenuItem>(model, nameof(MenuItem.ProductId));
			AssertHasFk<ClaimDefinition>(model, nameof(ClaimDefinition.ProductId));
			AssertHasFk<LdapGroupRoleMap>(model, nameof(LdapGroupRoleMap.OrganizationId));
			AssertHasFk<LdapGroupRoleMap>(model, nameof(LdapGroupRoleMap.RoleId));
			AssertHasFk<LdapGroupRoleMap>(model, nameof(LdapGroupRoleMap.ProductId));
			AssertHasFk<LdapGroupRoleMap>(model, nameof(LdapGroupRoleMap.BranchId));
			AssertHasFk<ExternalIdentityProvider>(model, nameof(ExternalIdentityProvider.OrganizationId));
			AssertHasFk<ClientProductBinding>(model, nameof(ClientProductBinding.ProductId));
			AssertHasFk<UserSession>(model, nameof(UserSession.UserId));
			AssertHasFk<UserSession>(model, nameof(UserSession.OrganizationId));
			AssertHasFk<UserSession>(model, nameof(UserSession.BranchId));
		}

		[TestMethod]
		public void Identity_Model_Should_Keep_Intentional_Weak_References()
		{
			using var context = IdentityDbContextExtensions.GetInMemoryIdentityDbContext(nameof(Identity_Model_Should_Keep_Intentional_Weak_References));
			var model = context.Model;

			AssertNoFk<AuthAuditEvent>(model, nameof(AuthAuditEvent.UserId));
			AssertNoFk<AuthAuditEvent>(model, nameof(AuthAuditEvent.ClientId));
			AssertNoFk<WebhookOutboxMessage>(model, nameof(WebhookOutboxMessage.ClientId));
			AssertNoFk<RevokedSession>(model, nameof(RevokedSession.SessionId));
			AssertNoFk<RevokedSession>(model, nameof(RevokedSession.UserId));
			AssertNoFk<RevokedSession>(model, nameof(RevokedSession.ClientId));
			AssertNoFk<ClientProductBinding>(model, nameof(ClientProductBinding.ClientId));
			AssertNoFk<UserSession>(model, nameof(UserSession.ClientId));
			AssertNoFk<ExternalIdentityProvider>(model, nameof(ExternalIdentityProvider.ClientId));
		}

		private static void AssertHasFk<TEntity>(IModel model, string foreignKeyProperty)
		{
			var entity = model.FindEntityType(typeof(TEntity));
			Assert.IsNotNull(entity, $"Entity type {typeof(TEntity).Name} missing from model.");

			var fk = entity.GetForeignKeys()
				.FirstOrDefault(x => x.Properties.Any(p => p.Name == foreignKeyProperty));
			Assert.IsNotNull(fk, $"{typeof(TEntity).Name}.{foreignKeyProperty} should have an explicit FK.");
			Assert.AreEqual(DeleteBehavior.Restrict, fk.DeleteBehavior, $"{typeof(TEntity).Name}.{foreignKeyProperty} should Restrict.");
		}

		private static void AssertNoFk<TEntity>(IModel model, string propertyName)
		{
			var entity = model.FindEntityType(typeof(TEntity));
			Assert.IsNotNull(entity, $"Entity type {typeof(TEntity).Name} missing from model.");

			var fk = entity.GetForeignKeys()
				.FirstOrDefault(x => x.Properties.Any(p => p.Name == propertyName));
			Assert.IsNull(fk, $"{typeof(TEntity).Name}.{propertyName} must remain a weak reference (D-00012).");
		}
	}
}
