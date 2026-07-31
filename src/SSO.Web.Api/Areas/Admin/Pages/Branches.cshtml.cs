using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Branches.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Branches.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class BranchesModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public BranchesModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<Branch> Items { get; set; } = new();

		[BindProperty(SupportsGet = true)]
		public Guid? EditId { get; set; }

		[BindProperty]
		public string Name { get; set; } = string.Empty;

		[BindProperty]
		public string Code { get; set; } = string.Empty;

		[BindProperty]
		public string? LegalName { get; set; }

		[BindProperty]
		public string? TradeName { get; set; }

		[BindProperty]
		public string? TaxId { get; set; }

		[BindProperty]
		public string? Segment { get; set; }

		[BindProperty]
		public string? Description { get; set; }

		[BindProperty]
		public string? PostalCode { get; set; }

		[BindProperty]
		public string? Street { get; set; }

		[BindProperty]
		public string? Number { get; set; }

		[BindProperty]
		public string? Complement { get; set; }

		[BindProperty]
		public string? City { get; set; }

		[BindProperty]
		public string? State { get; set; }

		[BindProperty]
		public Guid? ParentBranchId { get; set; }

		private bool CanManage => Portal.HasPermission(SsoAdminPermissions.Org) || Portal.IsPlatformAdmin;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			if (!RequireOrgContext())
			{
				return Page();
			}

			await LoadAsync();

			if (EditId is Guid id)
			{
				var item = Items.FirstOrDefault(x => x.Id == id);
				if (item is not null)
				{
					Name = item.Name;
					Code = item.Code;
					LegalName = item.LegalName;
					TradeName = item.TradeName;
					TaxId = item.TaxId;
					Segment = item.Segment;
					Description = item.Description;
					PostalCode = item.PostalCode;
					Street = item.Street;
					Number = item.Number;
					Complement = item.Complement;
					City = item.City;
					State = item.State;
					ParentBranchId = item.ParentBranchId;
				}
			}

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!CanManage)
			{
				return Forbid();
			}

			if (Portal.OrganizationId is not Guid orgId)
			{
				Error = "Selecione uma organização em Contexto.";
				return Page();
			}

			var cmd = AdminWrap.FromAnonymous<PostBranchCommand>(new
			{
				organizationId = orgId,
				parentBranchId = ParentBranchId,
				name = Name,
				code = Code,
				legalName = EmptyToNull(LegalName),
				tradeName = EmptyToNull(TradeName),
				taxId = EmptyToNull(TaxId),
				segment = EmptyToNull(Segment),
				description = EmptyToNull(Description),
				postalCode = EmptyToNull(PostalCode),
				street = EmptyToNull(Street),
				number = EmptyToNull(Number),
				complement = EmptyToNull(Complement),
				city = EmptyToNull(City),
				state = EmptyToNull(State)?.ToUpperInvariant()
			});
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Filial criada."))
			{
				ClearForm();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostUpdateAsync(Guid id)
		{
			if (!CanManage)
			{
				return Forbid();
			}

			if (Portal.OrganizationId is not Guid orgId)
			{
				Error = "Selecione uma organização em Contexto.";
				return Page();
			}

			var cmd = AdminWrap.FromAnonymous<PutBranchCommand>(new
			{
				id,
				organizationId = orgId,
				parentBranchId = ParentBranchId,
				name = Name,
				code = Code,
				legalName = EmptyToNull(LegalName),
				tradeName = EmptyToNull(TradeName),
				taxId = EmptyToNull(TaxId),
				segment = EmptyToNull(Segment),
				description = EmptyToNull(Description),
				postalCode = EmptyToNull(PostalCode),
				street = EmptyToNull(Street),
				number = EmptyToNull(Number),
				complement = EmptyToNull(Complement),
				city = EmptyToNull(City),
				state = EmptyToNull(State)?.ToUpperInvariant()
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Filial atualizada.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!CanManage)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteBranchCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Filial removida.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			var orgId = Portal.OrganizationId!.Value;
			Items = (await _reader.Query<Branch>().AsNoTracking()
					.Where(x => !x.IsDeleted && x.OrganizationId == orgId)
					.ToListAsync())
				.OrderBy(x => x.ParentBranchId.HasValue ? 1 : 0)
				.ThenBy(x => DigitsOnly(x.TaxId))
				.ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		private static string? EmptyToNull(string? value) =>
			string.IsNullOrWhiteSpace(value) ? null : value.Trim();

		private static string DigitsOnly(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return string.Empty;
			}

			return string.Concat(value.Where(char.IsDigit));
		}

		private void ClearForm()
		{
			Name = string.Empty;
			Code = string.Empty;
			LegalName = null;
			TradeName = null;
			TaxId = null;
			Segment = null;
			Description = null;
			PostalCode = null;
			Street = null;
			Number = null;
			Complement = null;
			City = null;
			State = null;
			ParentBranchId = null;
		}
	}
}
