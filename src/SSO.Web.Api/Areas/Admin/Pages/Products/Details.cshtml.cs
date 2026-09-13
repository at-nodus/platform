using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.MenuItems.Commands;
using SSO.Core.Application.Identity.Products.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ClaimDefinitions.Entity;
using SSO.Core.Domain.Identity.ClientProductBindings.Entity;
using SSO.Core.Domain.Identity.MenuItems.Entity;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.Products.Entity;
using SSO.Infrastructures.Data.Identity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;
using SSO.Web.Api.Areas.Admin;

namespace SSO.Web.Api.Areas.Admin.Pages.Products
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
		public Guid Id { get; set; }

		[BindProperty(SupportsGet = true)]
		public string Tab { get; set; } = "dados";

		public string ActiveTab => string.IsNullOrWhiteSpace(Tab) ? "dados" : Tab.ToLowerInvariant();

		public Product? Product { get; set; }
		public List<MenuItem> MenuItems { get; set; } = new();
		public List<Organization> EnabledOrganizations { get; set; } = new();
		public List<ClientProductBinding> Bindings { get; set; } = new();
		public List<ClaimDefinition> Claims { get; set; } = new();

		public bool CanManageMenus => Portal.IsPlatformAdmin || Portal.HasPermission(SsoAdminPermissions.Menus);

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public string MenuCode { get; set; } = string.Empty;

		[BindProperty]
		public string MenuTitle { get; set; } = string.Empty;

		[BindProperty]
		public string MenuRoute { get; set; } = string.Empty;

		[BindProperty]
		public string MenuPermissionCode { get; set; } = string.Empty;

		[BindProperty]
		public int MenuSortOrder { get; set; }

		[BindProperty]
		public string ClaimCode { get; set; } = string.Empty;

		[BindProperty]
		public string ClaimName { get; set; } = string.Empty;

		[BindProperty]
		public string ClaimValueType { get; set; } = ClaimValueTypes.String;

		[BindProperty]
		public string? ClaimDescription { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin && !CanManageMenus)
			{
				return Forbid();
			}

			await LoadAsync();
			return Product is null ? NotFound() : Page();
		}

		public async Task<IActionResult> OnPostUpdateAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "dados";
			var cmd = AdminWrap.FromAnonymous<PutProductCommand>(new { id = Id, name = Name, code = Code });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Produto atualizado.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostCreateMenuAsync()
		{
			if (!CanManageMenus)
			{
				return Forbid();
			}

			Tab = "menus";
			var cmd = AdminWrap.FromAnonymous<PostMenuItemCommand>(new
			{
				productId = Id,
				code = MenuCode,
				title = MenuTitle,
				route = MenuRoute,
				permissionCode = MenuPermissionCode,
				sortOrder = MenuSortOrder
			});
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Item de menu criado."))
			{
				MenuCode = MenuTitle = MenuRoute = MenuPermissionCode = string.Empty;
				MenuSortOrder = 0;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteMenuAsync(Guid menuId)
		{
			if (!CanManageMenus)
			{
				return Forbid();
			}

			Tab = "menus";
			var cmd = AdminWrap.FromAnonymous<DeleteMenuItemCommand>(new { id = menuId });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Item de menu removido.");
			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostCreateClaimAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "claims";
			try
			{
				var entity = ClaimDefinition.Create(ClaimCode.Trim().ToLowerInvariant(), ClaimName, ClaimValueType, Id, ClaimDescription);
				_db.ClaimDefinitions.Add(entity);
				await _db.SaveChangesAsync();
				Message = "Definição de claim criada.";
				ClaimCode = ClaimName = string.Empty;
				ClaimDescription = null;
			}
			catch (Exception ex)
			{
				Error = ex.Message;
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteClaimAsync(Guid claimId)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			Tab = "claims";
			var entity = await _db.ClaimDefinitions.FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == claimId && x.ProductId == Id);
			if (entity is null)
			{
				Error = "Definição não encontrada.";
			}
			else
			{
				entity.MarkDeleted();
				await _db.SaveChangesAsync();
				Message = "Definição removida.";
			}

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Product = await _reader.Query<Product>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id && !x.IsDeleted);
			if (Product is null)
			{
				return;
			}

			Name = Product.Name;
			Code = Product.Code;
			MenuItems = await _reader.Query<MenuItem>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.ProductId == Id)
				.OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
				.ToListAsync();
			EnabledOrganizations = await (
				from e in _reader.Query<ProductEnablement>().AsNoTracking()
				join o in _reader.Query<Organization>().AsNoTracking() on e.OrganizationId equals o.Id
				where !e.IsDeleted && !o.IsDeleted && e.ProductId == Id
				orderby o.Name
				select o).ToListAsync();
			Bindings = await _reader.Query<ClientProductBinding>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.ProductId == Id)
				.OrderBy(x => x.ClientId)
				.ToListAsync();
			Claims = await _reader.Query<ClaimDefinition>().AsNoTracking()
				.Where(x => !x.IsDeleted && x.ProductId == Id)
				.OrderBy(x => x.Code)
				.ToListAsync();
		}
	}
}
