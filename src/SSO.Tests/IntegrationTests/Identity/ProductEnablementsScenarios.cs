using System.Net;
using System.Text;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Tests.Helpers;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class ProductEnablementsScenarios
	{
		[TestMethod]
		public async Task POST_ProductEnablement_Should_Return_Created_For_PlatformAdmin()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreatePlatformAdminClient(server);

			var payload = new ProductEnablement
			{
				OrganizationId = IdentitySeed.DevOrganizationId,
				ProductId = Guid.NewGuid()
			};

			// Use a fresh product created via API first
			using var productClient = AdminAuthTestHelper.CreatePlatformAdminClient(server);
			var productResponse = await productClient.PostAsync(
				"/api/identity/products",
				new StringContent(JsonConvert.SerializeObject(new { name = "PE Test", code = $"pe-{Guid.NewGuid():N}".Substring(0, 12) }), Encoding.UTF8, "application/json"));
			Assert.AreEqual(HttpStatusCode.Created, productResponse.StatusCode, await productResponse.Content.ReadAsStringAsync());
			var productBody = await productResponse.Content.ReadAsStringAsync();
			dynamic productJson = JsonConvert.DeserializeObject(productBody)!;
			Guid productId = productJson.data.id;

			payload.ProductId = productId;

			var response = await client.PostAsync(
				"/api/identity/productenablements",
				new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, await response.Content.ReadAsStringAsync());
		}

		[TestMethod]
		public async Task POST_ProductEnablement_Should_Forbid_OrgAdmin()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreateOrgAdminClient(server, IdentitySeed.DevOrganizationId);

			var response = await client.PostAsync(
				"/api/identity/productenablements",
				new StringContent(JsonConvert.SerializeObject(new
				{
					organizationId = IdentitySeed.DevOrganizationId,
					productId = IdentitySeed.DevProductId
				}), Encoding.UTF8, "application/json"));

			Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
		}

		[TestMethod]
		public async Task GET_ProductEnablements_Should_Allow_OrgAdmin()
		{
			using var server = ServerHelper.Create();
			using var client = AdminAuthTestHelper.CreateOrgAdminClient(server, IdentitySeed.DevOrganizationId);

			var response = await client.GetAsync("/api/identity/productenablements");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, await response.Content.ReadAsStringAsync());
		}
	}
}
