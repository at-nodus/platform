using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.ProductEnablements;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Shared.Identity;

namespace SSO.Infrastructures.Services.Identity
{
	public sealed class ProductEnablementGuard : IProductEnablementGuard
	{
		private readonly IIdentityDbContextReader _reader;

		public ProductEnablementGuard(IIdentityDbContextReader reader)
		{
			_reader = reader;
		}

		public async Task<bool> IsEnabledAsync(Guid organizationId, Guid productId, CancellationToken cancellationToken = default)
		{
			return await _reader.Query<ProductEnablement>().AsNoTracking()
				.AnyAsync(
					x => !x.IsDeleted
						&& x.OrganizationId == organizationId
						&& x.ProductId == productId,
					cancellationToken);
		}

		public async Task EnsureEnabledForClientAsync(Guid organizationId, string? clientId, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				return;
			}

			var productId = await _reader.Query<ClientProductBinding>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.ClientId == clientId)
				.Select(x => (Guid?)x.ProductId)
				.FirstOrDefaultAsync(cancellationToken);

			if (productId is null)
			{
				return;
			}

			var productCode = await _reader.Query<Product>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.Id == productId.Value)
				.Select(x => x.Code)
				.FirstOrDefaultAsync(cancellationToken);

			if (string.Equals(productCode, SsoProductCodes.Platform, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (!await IsEnabledAsync(organizationId, productId.Value, cancellationToken))
			{
				throw new ProductNotEnabledForOrganizationException(organizationId, productId.Value);
			}
		}
	}
}
