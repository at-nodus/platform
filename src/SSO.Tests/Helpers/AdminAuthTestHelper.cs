using System.Net;
using Microsoft.AspNetCore.TestHost;
using SSO.Infrastructures.Data.Identity;
using SSO.Shared.Identity;

namespace SSO.Tests.Helpers
{
	public static class AdminAuthTestHelper
	{
		public static HttpClient CreateAuthenticatedClient(
			TestServer server,
			params string[] permissions)
			=> CreateAuthenticatedClient(server, organizationId: null, permissions);

		public static HttpClient CreateAuthenticatedClient(
			TestServer server,
			Guid? organizationId,
			params string[] permissions)
		{
			var client = server.CreateClient();
			ApplyAuthHeaders(client, organizationId, permissions);
			return client;
		}

		public static void ApplyAuthHeaders(
			HttpClient client,
			Guid? organizationId,
			params string[] permissions)
		{
			client.DefaultRequestHeaders.Remove(SsoTestingAuthDefaults.PermissionsHeader);
			client.DefaultRequestHeaders.Remove(SsoTestingAuthDefaults.OrganizationIdHeader);
			client.DefaultRequestHeaders.Remove(SsoTestingAuthDefaults.UserIdHeader);

			client.DefaultRequestHeaders.Add(
				SsoTestingAuthDefaults.PermissionsHeader,
				string.Join(",", permissions));

			if (organizationId is Guid orgId)
			{
				client.DefaultRequestHeaders.Add(
					SsoTestingAuthDefaults.OrganizationIdHeader,
					orgId.ToString("D"));
			}
		}

		public static HttpClient CreatePlatformAdminClient(TestServer server)
			=> CreateAuthenticatedClient(
				server,
				organizationId: null,
				SsoAdminPermissions.Platform,
				SsoAdminPermissions.Org,
				SsoAdminPermissions.AuditRead,
				SsoAdminPermissions.SessionsRevoke,
				SsoAdminPermissions.Menus);

		public static HttpClient CreateOrgAdminClient(TestServer server, Guid organizationId)
			=> CreateAuthenticatedClient(
				server,
				organizationId,
				SsoAdminPermissions.Org,
				SsoAdminPermissions.SessionsRevoke);

		/// <summary>
		/// Cookie login for Razor Areas (/Admin, /Me). Test permission headers only authenticate APIs.
		/// </summary>
		public static async Task<HttpClient> CreateLoggedInPortalClientAsync(TestServer server)
		{
			var client = new HttpClient(new CookieJarHandler(server.CreateHandler()))
			{
				BaseAddress = server.BaseAddress
			};

			var login = await client.PostAsync(
				"/Account/Login?returnUrl=%2FAdmin",
				new FormUrlEncodedContent(new Dictionary<string, string>
				{
					["Input.Email"] = IdentitySeed.DevUserEmail,
					["Input.Password"] = IdentitySeed.DevUserPassword
				}));

			if (login.StatusCode is not (HttpStatusCode.Redirect or HttpStatusCode.Found or HttpStatusCode.OK))
			{
				throw new InvalidOperationException(
					$"Portal login failed: {(int)login.StatusCode} {login.StatusCode}. Body={await login.Content.ReadAsStringAsync()}");
			}

			return client;
		}

		private sealed class CookieJarHandler : DelegatingHandler
		{
			private readonly List<string> _pairs = new();

			public CookieJarHandler(HttpMessageHandler inner) : base(inner)
			{
			}

			protected override async Task<HttpResponseMessage> SendAsync(
				HttpRequestMessage request,
				CancellationToken cancellationToken)
			{
				if (_pairs.Count > 0)
				{
					request.Headers.Remove("Cookie");
					request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", _pairs));
				}

				var response = await base.SendAsync(request, cancellationToken);
				if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
				{
					foreach (var setCookie in setCookies)
					{
						var pair = setCookie.Split(';', 2)[0].Trim();
						if (string.IsNullOrEmpty(pair) || !pair.Contains('='))
						{
							continue;
						}

						var name = pair.Split('=', 2)[0];
						_pairs.RemoveAll(p => p.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase));
						_pairs.Add(pair);
					}
				}

				return response;
			}
		}
	}
}
