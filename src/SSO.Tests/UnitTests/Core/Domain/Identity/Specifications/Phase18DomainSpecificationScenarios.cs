using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.MenuItems.Specifications;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using SSO.Core.Domain.Identity.OrganizationContacts.Specifications;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.Permissions.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Entity;
using SSO.Core.Domain.Identity.UserRoleAssignments.Specifications;
using SSO.Tests.Helpers.Data.Identity;

namespace SSO.Tests.UnitTests.Core.Domain.Identity.Specifications
{
	[TestClass]
	public class Phase18DomainSpecificationScenarios
	{
		[TestMethod]
		public void UserRoleAssignment_Branch_From_Other_Org_Should_Be_Invalid()
		{
			using var context = IdentityDbContextExtensions.GetInMemoryIdentityDbContext(nameof(UserRoleAssignment_Branch_From_Other_Org_Should_Be_Invalid));
			var reader = context.GetDbContextReader();

			var orgA = new Organization { Id = Guid.NewGuid(), Name = "A", Code = "org-a" };
			orgA.MarkCreated();
			var orgB = new Organization { Id = Guid.NewGuid(), Name = "B", Code = "org-b" };
			orgB.MarkCreated();
			var branchB = new Branch { Id = Guid.NewGuid(), OrganizationId = orgB.Id, Name = "HQ", Code = "hq" };
			branchB.MarkCreated();
			context.Organizations.AddRange(orgA, orgB);
			context.Branches.Add(branchB);
			context.SaveChanges();

			var spec = new UserRoleAssignmentBranchDoesNotBelongToOrganizationSpecification(reader);
			var compiled = spec.ToExpression().Compile();

			var invalid = new UserRoleAssignment
			{
				OrganizationId = orgA.Id,
				BranchId = branchB.Id
			};
			Assert.IsTrue(compiled(invalid));

			var valid = new UserRoleAssignment
			{
				OrganizationId = orgB.Id,
				BranchId = branchB.Id
			};
			Assert.IsFalse(compiled(valid));

			var orgWide = new UserRoleAssignment { OrganizationId = orgA.Id, BranchId = null };
			Assert.IsFalse(compiled(orgWide));
		}

		[TestMethod]
		public void MenuItem_PermissionCode_Must_Exist()
		{
			using var context = IdentityDbContextExtensions.GetInMemoryIdentityDbContext(nameof(MenuItem_PermissionCode_Must_Exist));
			var reader = context.GetDbContextReader();

			var permission = new Permission { Code = "sso.admin.org", Name = "Org admin" };
			permission.MarkCreated();
			context.Set<Permission>().Add(permission);
			context.SaveChanges();

			var spec = new MenuItemPermissionCodeDoesNotExistSpecification(reader);
			var compiled = spec.ToExpression().Compile();

			Assert.IsTrue(compiled(new MenuItem { PermissionCode = "missing.permission" }));
			Assert.IsFalse(compiled(new MenuItem { PermissionCode = "sso.admin.org" }));
		}

		[TestMethod]
		public void OrganizationContact_Second_Primary_Should_Be_Invalid()
		{
			using var context = IdentityDbContextExtensions.GetInMemoryIdentityDbContext(nameof(OrganizationContact_Second_Primary_Should_Be_Invalid));
			var reader = context.GetDbContextReader();

			var org = new Organization { Id = Guid.NewGuid(), Name = "Org", Code = "org-c" };
			org.MarkCreated();
			var existing = new OrganizationContact
			{
				Id = Guid.NewGuid(),
				OrganizationId = org.Id,
				Name = "Primary",
				IsPrimary = true
			};
			existing.MarkCreated();
			context.Organizations.Add(org);
			context.OrganizationContacts.Add(existing);
			context.SaveChanges();

			var spec = new OrganizationContactPrimaryAlreadyExistsSpecification(reader);
			var compiled = spec.ToExpression().Compile();

			var second = new OrganizationContact
			{
				Id = Guid.NewGuid(),
				OrganizationId = org.Id,
				Name = "Other",
				IsPrimary = true
			};
			Assert.IsTrue(compiled(second));

			var secondary = new OrganizationContact
			{
				OrganizationId = org.Id,
				Name = "Secondary",
				IsPrimary = false
			};
			Assert.IsFalse(compiled(secondary));
		}
	}
}
