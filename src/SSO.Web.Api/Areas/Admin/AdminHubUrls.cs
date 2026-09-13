using System;

namespace SSO.Web.Api.Areas.Admin
{
	/// <summary>Canonical Admin hub URLs (feature 00016). Child cadastros live under the parent, not in the sidebar.</summary>
	public static class AdminHubUrls
	{
		public const string Organizations = "/Admin/Organizations";
		public const string Products = "/Admin/Products";
		public const string Roles = "/Admin/Roles";
		public const string Users = "/Admin/Users";
		public const string Permissions = "/Admin/Permissions";
		public const string AuthClients = "/Admin/AuthClients";
		public const string SwitchContext = "/Admin/SwitchContext";
		public const string GlobalClaims = "/Admin/Products/Claims";

		public static string OrganizationDetails(Guid id) => $"/Admin/Organizations/Details/{id:D}";

		public static string Organization(Guid id, string tab) => tab switch
		{
			"branches" => $"{OrganizationDetails(id)}#branches",
			"contato" => $"{OrganizationDetails(id)}#contato",
			"produtos" => $"{OrganizationDetails(id)}#produtos",
			"usuarios" => $"{OrganizationDetails(id)}#usuarios",
			"invites" => $"/Admin/Organizations/{id:D}/Invites",
			"assignments" => $"/Admin/Organizations/{id:D}/Assignments",
			"userclaims" => $"/Admin/Organizations/{id:D}/UserClaims",
			"ldap" => $"/Admin/Organizations/{id:D}/LdapMaps",
			"idps" => $"/Admin/Organizations/{id:D}/IdPs",
			_ => OrganizationDetails(id)
		};

		public static string ProductDetails(Guid id, string tab = "dados")
		{
			var baseUrl = $"/Admin/Products/Details/{id:D}";
			return tab is "dados" or "" ? baseUrl : $"{baseUrl}?tab={Uri.EscapeDataString(tab)}";
		}

		public static string RoleDetails(Guid id, string tab = "dados")
		{
			var baseUrl = $"/Admin/Roles/Details/{id:D}";
			return tab is "dados" or "" ? baseUrl : $"{baseUrl}?tab={Uri.EscapeDataString(tab)}";
		}

		public static string UserDetails(Guid id, string tab = "dados")
		{
			var baseUrl = $"/Admin/Users/Details/{id:D}";
			return tab is "dados" or "" ? baseUrl : $"{baseUrl}?tab={Uri.EscapeDataString(tab)}";
		}

		public static string PermissionDetails(Guid id, string tab = "dados")
		{
			var baseUrl = $"/Admin/Permissions/Details/{id:D}";
			return tab is "dados" or "" ? baseUrl : $"{baseUrl}?tab={Uri.EscapeDataString(tab)}";
		}

		public static string AuthClientDetails(string clientId, string tab = "dados")
		{
			var baseUrl = $"/Admin/AuthClients/Details/{Uri.EscapeDataString(clientId)}";
			return tab is "dados" or "" ? baseUrl : $"{baseUrl}?tab={Uri.EscapeDataString(tab)}";
		}
	}
}
