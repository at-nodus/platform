using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Services;
using SSO.Core.Domain.Identity.ProductEnablements.Specifications;
using SSO.Core.Domain.Identity.ProductEnablements.Validations.DomainValidations;
using SSO.Core.Domain.Identity.ProductEnablements.Validations.EntityValidations;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Tests.Helpers;
using SSO.Tests.Helpers.Data.Identity;

namespace SSO.Tests.UnitTests.Core.Domain.Identity.ProductEnablements.Services
{
	[TestClass]
	public class CreateProductEnablementServiceScenarios
	{
		[TestMethod]
		public async Task CreateProductEnablement_Should_Persist_Entity()
		{
			using var context = IdentityDbContextExtensions.GetInMemoryIdentityDbContext(nameof(CreateProductEnablement_Should_Persist_Entity));
			var reader = context.GetDbContextReader();
			var writer = context.GetDbContextWriter();

			var org = new Organization { Name = "Org", Code = "org-pe" };
			org.MarkCreated();
			var product = new Product { Name = "App", Code = "app-pe" };
			product.MarkCreated();
			await writer.AddAsync(org);
			await writer.AddAsync(product);
			await writer.CommitAsync();

			var handler = new CreateProductEnablementServiceRequestHandler(
				writer,
				GenericHelper.CreateLocalizer<ProductEnablement>(),
				new ProductEnablementValidator(),
				new CreateProductEnablementSpecificationsValidator(
					new ProductEnablementOrganizationProductAlreadyExistsSpecification(reader)));

			var enablement = new ProductEnablement { OrganizationId = org.Id, ProductId = product.Id };
			enablement.MarkCreated();

			var result = await handler.Handle(new CreateProductEnablementServiceRequest(enablement), default);
			await writer.CommitAsync();

			Assert.AreEqual(org.Id, result.OrganizationId);
			Assert.AreEqual(1, context.ProductEnablements.Count(x => !x.IsDeleted));
		}

		[TestMethod]
		public async Task CreateProductEnablement_Should_Reject_Duplicate_Org_Product()
		{
			using var context = IdentityDbContextExtensions.GetInMemoryIdentityDbContext(nameof(CreateProductEnablement_Should_Reject_Duplicate_Org_Product));
			var reader = context.GetDbContextReader();
			var writer = context.GetDbContextWriter();

			var org = new Organization { Name = "Org2", Code = "org-pe2" };
			org.MarkCreated();
			var product = new Product { Name = "App2", Code = "app-pe2" };
			product.MarkCreated();
			await writer.AddAsync(org);
			await writer.AddAsync(product);
			await writer.CommitAsync();

			var handler = new CreateProductEnablementServiceRequestHandler(
				writer,
				GenericHelper.CreateLocalizer<ProductEnablement>(),
				new ProductEnablementValidator(),
				new CreateProductEnablementSpecificationsValidator(
					new ProductEnablementOrganizationProductAlreadyExistsSpecification(reader)));

			var first = new ProductEnablement { OrganizationId = org.Id, ProductId = product.Id };
			first.MarkCreated();
			await handler.Handle(new CreateProductEnablementServiceRequest(first), default);
			await writer.CommitAsync();

			var duplicate = new ProductEnablement { OrganizationId = org.Id, ProductId = product.Id };
			duplicate.MarkCreated();

			try
			{
				await handler.Handle(new CreateProductEnablementServiceRequest(duplicate), default);
				Assert.Fail("Expected domain validation failure for duplicate enablement.");
			}
			catch (Exception ex)
			{
				Assert.IsFalse(ex is AssertFailedException);
			}
		}
	}
}
