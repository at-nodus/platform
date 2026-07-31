using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SSO.Core.Application.Identity.Organizations.Commands;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.Organizations.Entity;
using SSO.Middleware.Identity;
using SSO.Shared.Identity;

namespace SSO.Web.Api.Areas.Admin.Pages
{
	public sealed class OrganizationsModel : AdminPageModel
	{
		private readonly IIdentityDbContextReader _reader;
		private readonly IMediator _mediator;

		public OrganizationsModel(IAdminPortalContextService portal, IIdentityDbContextReader reader, IMediator mediator) : base(portal)
		{
			_reader = reader;
			_mediator = mediator;
		}

		public List<Organization> Items { get; set; } = new();

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
		public string BranchAuthzInheritance { get; set; } = BranchAuthzInheritancePolicies.Off;

		public async Task<IActionResult> OnGetAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
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
					BranchAuthzInheritance = item.BranchAuthzInheritance;
				}
			}

			return Page();
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PostOrganizationCommand>(new
			{
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
				state = EmptyToNull(State)?.ToUpperInvariant(),
				branchAuthzInheritance = BranchAuthzInheritance
			});
			var response = await _mediator.Send(cmd);
			if (ApplyResponse(response, "Organização criada."))
			{
				ClearForm();
			}

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostUpdateAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<PutOrganizationCommand>(new
			{
				id,
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
				state = EmptyToNull(State)?.ToUpperInvariant(),
				branchAuthzInheritance = BranchAuthzInheritance
			});
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Organização atualizada.");

			await LoadAsync();
			return Page();
		}

		public async Task<IActionResult> OnPostDeleteAsync(Guid id)
		{
			if (!Portal.IsPlatformAdmin)
			{
				return Forbid();
			}

			var cmd = AdminWrap.FromAnonymous<DeleteOrganizationCommand>(new { id });
			var response = await _mediator.Send(cmd);
			ApplyResponse(response, "Organização removida.");

			await LoadAsync();
			return Page();
		}

		private async Task LoadAsync()
		{
			Items = await _reader.Query<Organization>().AsNoTracking()
				.Where(x => !x.IsDeleted)
				.OrderBy(x => x.Name)
				.ToListAsync();
		}

		private static string? EmptyToNull(string? value) =>
			string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
			BranchAuthzInheritance = BranchAuthzInheritancePolicies.Off;
		}
	}
}
