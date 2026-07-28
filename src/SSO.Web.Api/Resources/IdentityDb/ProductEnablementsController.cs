using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.ProductEnablements.Commands;
using SSO.Core.Application.Identity.ProductEnablements.Queries;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/productenablements")]
	public sealed class ProductEnablementsController : ResourceController
	{
		[HttpGet]
		[RequiresPermission(SsoAdminPermissions.Org, SsoAdminPermissions.Platform)]
		public async Task<ActionResult<GetProductEnablementsByFilterQueryResponse>> Get(GetProductEnablementsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		[RequiresPermission(SsoAdminPermissions.Org, SsoAdminPermissions.Platform)]
		public async Task<ActionResult<GetProductEnablementByIdQueryResponse>> Get(GetProductEnablementByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<ActionResult<PostProductEnablementCommandResponse>> Post(PostProductEnablementCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		[RequiresPermission(SsoAdminPermissions.Platform)]
		public async Task<ActionResult<DeleteProductEnablementCommandResponse>> Delete(DeleteProductEnablementCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
