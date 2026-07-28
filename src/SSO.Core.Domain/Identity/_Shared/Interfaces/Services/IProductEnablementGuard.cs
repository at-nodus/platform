using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Domain.Identity._Context.Interfaces.Services
{
	public interface IProductEnablementGuard
	{
		Task<bool> IsEnabledAsync(Guid organizationId, Guid productId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Fail-closed commercial gate when organization context is present and the client maps to a product.
		/// No-ops when client has no binding or product is platform-exempt (<c>sso-platform</c>).
		/// </summary>
		Task EnsureEnabledForClientAsync(Guid organizationId, string? clientId, CancellationToken cancellationToken = default);
	}
}
