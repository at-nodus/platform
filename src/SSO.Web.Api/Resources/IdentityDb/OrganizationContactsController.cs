using Microsoft.AspNetCore.Mvc;
using SSO.Core.Application.Identity.OrganizationContacts.Commands;
using SSO.Core.Application.Identity.OrganizationContacts.Queries;
using SSO.Middleware.Identity.Authorization;
using SSO.Shared.Identity;
using SSO.Web.Api.Abstractions.Controllers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Web.Api.Identity
{
	[Produces("application/json")]
	[Route("api/identity/organization-contacts")]
	[RequiresPermission(SsoAdminPermissions.Org, SsoAdminPermissions.Platform)]
	public sealed class OrganizationContactsController : ResourceController
	{
		[HttpGet]
		public async Task<ActionResult<GetOrganizationContactsByFilterQueryResponse>> Get(GetOrganizationContactsByFilterQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpGet("{id:Guid}")]
		public async Task<ActionResult<GetOrganizationContactByIdQueryResponse>> Get(GetOrganizationContactByIdQuery request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPost]
		public async Task<ActionResult<PostOrganizationContactCommandResponse>> Post(PostOrganizationContactCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpPut("{id:Guid}")]
		public async Task<ActionResult<PutOrganizationContactCommandResponse>> Put(PutOrganizationContactCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);

		[HttpDelete("{id:Guid}")]
		public async Task<ActionResult<DeleteOrganizationContactCommandResponse>> Delete(DeleteOrganizationContactCommand request, CancellationToken cancellationToken = default)
			=> await Send(request, cancellationToken);
	}
}
