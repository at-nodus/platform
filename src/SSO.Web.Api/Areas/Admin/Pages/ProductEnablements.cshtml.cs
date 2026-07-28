using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.ProductEnablements.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class ProductEnablementsModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public ProductEnablementsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<ProductEnablement> Items { get; set; } = new();
		public List<Product> Products { get; set; } = new();
		public List<Organization> Organizations { get; set; } = new();
		public bool CanWrite => Portal.IsPlatformAdmin;
		public bool CanRead => Portal.IsPlatformAdmin || Portal.HasPermission(SsoAdminPermissions.Org);

		[BindProperty]
		public Guid OrganizationId { get; set; }

		[BindProperty]
		public Guid ProductId { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanRead)
			{
				return Forbid();
			}

			if (!Portal.IsPlatformAdmin && !RequireOrgContext())
			{
				return Page();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!CanWrite)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PostProductEnablementCommand>(new { organizationId = OrganizationId, productId = ProductId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Produto habilitado para a organização.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!CanWrite)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteProductEnablementCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Habilitação removida.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Products = await _reader.Query<Product>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Name)
				.ToListAsync();

			Organizations = await _reader.Query<Organization>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Name)
				.ToListAsync();

			var query = _reader.Query<ProductEnablement>().AsNoTracking().Where(x => !x.IsDeleted);
			if (!Portal.IsPlatformAdmin)
			{
				var orgId = Portal.OrganizationId!.Value;
				query = query.Where(x => x.OrganizationId == orgId);
			}

			Items = await query.OrderBy(x => x.CreatedAt).ToListAsync();
		}
	}
}
