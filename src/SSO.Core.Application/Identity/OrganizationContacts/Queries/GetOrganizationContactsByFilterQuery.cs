using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.FullSearch;
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
	public sealed class GetOrganizationContactsByFilterQuery : ApplicationRequest<OrganizationContact, GetOrganizationContactsByFilterQueryResponse>
	{
		public GetOrganizationContactsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetOrganizationContactsByFilterQueryResponse : ApplicationResponse<OrganizationContact>
	{
		public GetOrganizationContactsByFilterQueryResponse(Tuple<int, int, WrapRequest<OrganizationContact>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetOrganizationContactsByFilterQueryResponse(int statusCode, WrapRequest<OrganizationContact> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetOrganizationContactsByFilterQueryHandler : ApplicationRequestHandler<OrganizationContact, GetOrganizationContactsByFilterQuery, GetOrganizationContactsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }
		private ICurrentAdminContext AdminContext { get; set; }

		public GetOrganizationContactsByFilterQueryHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<OrganizationContact> localizer,
			IIdentityDbContextReader reader,
			ICurrentAdminContext adminContext)
		{
			Logger = logger; Localizer = localizer; Reader = reader; AdminContext = adminContext;
		}

		override public async Task<GetOrganizationContactsByFilterQueryResponse> Handle(GetOrganizationContactsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var query = Reader.Query<OrganizationContact>().AsNoTracking().Where(x => !x.IsDeleted);

				if (!AdminContext.IsPlatformAdmin)
				{
					if (AdminContext.OrganizationId is not Guid orgId)
					{
						return new GetOrganizationContactsByFilterQueryResponse((int)HttpStatusCode.OK, request, Array.Empty<OrganizationContact>(), Localizer["Successful operation!"], 0);
					}

					query = query.Where(x => x.OrganizationId == orgId);
				}

				var data = await query.FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetOrganizationContactsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetOrganizationContactsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetOrganizationContactsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
