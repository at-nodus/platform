using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Select;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationContacts.Queries
{
	public sealed class GetOrganizationContactByIdQuery : ApplicationRequest<OrganizationContact, GetOrganizationContactByIdQueryResponse>
	{
		public GetOrganizationContactByIdQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class GetOrganizationContactByIdQueryResponse : ApplicationResponse<OrganizationContact>
	{
		public GetOrganizationContactByIdQueryResponse(Tuple<int, int, WrapRequest<OrganizationContact>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetOrganizationContactByIdQueryResponse(int statusCode, WrapRequest<OrganizationContact> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetOrganizationContactByIdQueryHandler : ApplicationRequestHandler<OrganizationContact, GetOrganizationContactByIdQuery, GetOrganizationContactByIdQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }
		private ICurrentAdminContext AdminContext { get; set; }

		public GetOrganizationContactByIdQueryHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<OrganizationContact> localizer,
			IIdentityDbContextReader reader,
			ICurrentAdminContext adminContext)
		{
			Logger = logger; Localizer = localizer; Reader = reader; AdminContext = adminContext;
		}

		override public async Task<GetOrganizationContactByIdQueryResponse> Handle(GetOrganizationContactByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var id = request.Project(x => x.Id);
				var organizationId = await Reader.Query<OrganizationContact>().AsNoTracking()
					.Where(x => x.Id == id && !x.IsDeleted)
					.Select(x => (Guid?)x.OrganizationId)
					.SingleOrDefaultAsync(cancellationToken);
				if (organizationId is null) throw new EntityNotFoundException<OrganizationContact>(Localizer);

				AdminContext.EnsureCanAccessOrganization(organizationId.Value);

				var data = await Reader.Query<OrganizationContact>().Where(x => x.Id == id && !x.IsDeleted).Select(request).SingleOrDefaultAsync(cancellationToken);
				if (data == null) throw new EntityNotFoundException<OrganizationContact>(Localizer);

				return new GetOrganizationContactByIdQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetOrganizationContactByIdQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetOrganizationContactByIdQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
