using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.ClientProductBindings.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.AuthClientMetadata.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.AuthClients
{
	public sealed class DetailsModel : AdminHubPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;
		private readonly IdentityDbContext _db;

		public DetailsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator, IdentityDbContext db) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
			_db = db;
		}

		[BindProperty(SupportsGet = true)]
		public string ClientId { get; set; } = string.Empty;

		[BindProperty(SupportsGet = true)]
		public string Tab { get; set; } = "dados";

		public string ActiveTab => string.IsNullOrWhiteSpace(Tab) ? "dados" : Tab.ToLowerInvariant();

		public AuthClientMetadataEntity? Metadata { get; set; }
		public List<ClientProductBinding> Bindings { get; set; } = new();
		public List<Product> AllProducts { get; set; } = new();
		public List<Product> AvailableProducts { get; set; } = new();

		[BindProperty]
		public Guid ProductId { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			await LoadAsync();
			return Metadata is null ? NotFound() : Page();
		}

		public async Task<IActionResult> OnPostDisableAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "dados";
			var meta = await _db.AuthClientMetadata.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == ClientId);
			if (meta is null)
			{
				Error = "Client não encontrado.";
			}
			else
			{
				meta.IsEnabled = false;
				meta.TouchUpdated();
				await _db.SaveChangesAsync();
				Message = "Client desabilitado.";
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostAddBindingAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "bindings";
			var cmd = AdminWrap.FromAnonymous<PostClientProductBindingCommand>(new { clientId = ClientId, productId = ProductId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Vínculo criado.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostRemoveBindingAsync(Guid bindingId)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "bindings";
			var cmd = AdminWrap.FromAnonymous<DeleteClientProductBindingCommand>(new { id = bindingId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Vínculo removido.");
			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Metadata = await _reader.Query<AuthClientMetadataEntity>().AsNoTracking()
				.FirstOrDefaultAsync(x => !x.IsDeleted && x.ClientId == ClientId);
			if (Metadata is null)
			{
				return;
			}

			AllProducts = await _reader.Query<Product>().AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
			Bindings = await _reader.Query<ClientProductBinding>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.ClientId == ClientId)
				.OrderBy(x => x.ClientId)
				.ToListAsync();
			var bound = Bindings.Select(x => x.ProductId).ToHashSet();
			AvailableProducts = AllProducts.Where(x => !bound.Contains(x.Id)).ToList();
		}
	}
}
