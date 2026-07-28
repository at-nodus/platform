using BAYSOFT.Abstractions.Crosscutting.InheritStringLocalization;
using SSO.Core.Domain.Identity._Shared;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Resources;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Core.Domain.Resources;
using System;

namespace SSO.Core.Domain.Identity.ProductEnablements.Entity
{
	[InheritStringLocalizer(typeof(Messages), Priority = 1)]
	[InheritStringLocalizer(typeof(EntityProductEnablement), Priority = 0)]
	public sealed class ProductEnablement : IdentityAuditableEntity
	{
		public Guid OrganizationId { get; set; }
		public Guid ProductId { get; set; }

		public Organization Organization { get; set; }
		public Product Product { get; set; }

		public ProductEnablement()
		{
		}
	}
}
