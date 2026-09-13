using System.Net;
using Microsoft.AspNetCore.TestHost;
using SSO.Infrastructures.Data.Identity;
using SSO.Tests.Helpers;

namespace SSO.Tests.IntegrationTests.Identity
{
	[TestClass]
	public class AdminHubNavigationScenarios
	{
		private static Task<HttpClient> CreatePortalClientAsync(TestServer server)
			=> AdminAuthTestHelper.CreateLoggedInPortalClientAsync(server);

		[TestMethod]
		public async Task PlatformAdmin_Without_Session_Legacy_Branches_Goes_To_Organizations()
		{
			using var server = ServerHelper.Create();
			using var client = await CreatePortalClientAsync(server);

			var response = await client.GetAsync("/Admin/Branches");

			Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
			var location = response.Headers.Location?.ToString() ?? string.Empty;
			StringAssert.Contains(location, "/Admin/Organizations");
		}

		[TestMethod]
		public async Task PlatformAdmin_Can_Open_Organization_Invites_Hub()
		{
			using var server = ServerHelper.Create();
			using var client = await CreatePortalClientAsync(server);

			var response = await client.GetAsync($"/Admin/Organizations/{IdentitySeed.DevOrganizationId:D}/Invites");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			var html = await response.Content.ReadAsStringAsync();
			StringAssert.Contains(html, "Convites");
		}

		[TestMethod]
		public async Task Me_Organization_Details_Does_Not_Show_Admin_Tabs()
		{
			using var server = ServerHelper.Create();
			using var client = await CreatePortalClientAsync(server);

			var response = await client.GetAsync($"/Me/Organizations/Details/{IdentitySeed.DevOrganizationId:D}");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			var html = await response.Content.ReadAsStringAsync();
			StringAssert.Contains(html, "Branches");
			Assert.IsFalse(html.Contains($"/Admin/Organizations/{IdentitySeed.DevOrganizationId:D}/Invites"), html);
		}

		[TestMethod]
		public async Task Admin_Organization_Details_Shows_Nested_Admin_Tabs()
		{
			using var server = ServerHelper.Create();
			using var client = await CreatePortalClientAsync(server);

			var response = await client.GetAsync($"/Admin/Organizations/Details/{IdentitySeed.DevOrganizationId:D}");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			var html = await response.Content.ReadAsStringAsync();
			StringAssert.Contains(html, $"/Admin/Organizations/{IdentitySeed.DevOrganizationId:D}/Invites");
			StringAssert.Contains(html, "Acessos");
		}

		[TestMethod]
		public async Task PlatformAdmin_Legacy_MenuItems_Redirects_To_Products()
		{
			using var server = ServerHelper.Create();
			using var client = await CreatePortalClientAsync(server);

			var response = await client.GetAsync("/Admin/MenuItems");

			Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
			var location = response.Headers.Location?.ToString() ?? string.Empty;
			StringAssert.Contains(location, "/Admin/Products");
		}

		[TestMethod]
		public async Task PlatformAdmin_Can_Open_Role_Hub()
		{
			using var server = ServerHelper.Create();
			using var client = await CreatePortalClientAsync(server);

			var response = await client.GetAsync($"/Admin/Roles/Details/{IdentitySeed.DevRoleOrgAdminId}?tab=permissions");

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			var html = await response.Content.ReadAsStringAsync();
			StringAssert.Contains(html, "Permissões");
		}

		[TestMethod]
		public async Task MyOrganization_Without_Context_Goes_To_SwitchContext()
		{
			using var server = ServerHelper.Create();
			using var client = await CreatePortalClientAsync(server);

			var response = await client.GetAsync("/Admin/MyOrganization");

			Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
			var location = response.Headers.Location?.ToString() ?? string.Empty;
			StringAssert.Contains(location, "SwitchContext");
		}

		[TestMethod]
		public async Task Admin_Home_Sidebar_Lists_Roots_Not_Child_Cadastros()
		{
			using var server = ServerHelper.Create();
			using var client = await CreatePortalClientAsync(server);

			var response = await client.GetAsync("/Admin");
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			var html = await response.Content.ReadAsStringAsync();
			StringAssert.Contains(html, "Organizações");
			StringAssert.Contains(html, "Minha organização");
			Assert.IsFalse(html.Contains("href=\"/Admin/Branches\""), html);
			Assert.IsFalse(html.Contains("href=\"/Admin/Invites\""), html);
			Assert.IsFalse(html.Contains("href=\"/Admin/MenuItems\""), html);
		}
	}
}
