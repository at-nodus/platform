using System;

namespace SSO.Core.Domain.Identity.ProductEnablements
{
	public sealed class ProductNotEnabledForOrganizationException : Exception
	{
		public const string ErrorCode = "product_not_enabled_for_organization";

		public Guid OrganizationId { get; }
		public Guid ProductId { get; }

		public ProductNotEnabledForOrganizationException(Guid organizationId, Guid productId)
			: base("The product is not enabled for the organization.")
		{
			OrganizationId = organizationId;
			ProductId = productId;
		}
	}
}
