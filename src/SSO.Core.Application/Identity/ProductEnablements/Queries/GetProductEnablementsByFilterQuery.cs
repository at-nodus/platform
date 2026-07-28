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
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ProductEnablements.Queries
{
	public sealed class GetProductEnablementsByFilterQuery : ApplicationRequest<ProductEnablement, GetProductEnablementsByFilterQueryResponse>
	{
		public GetProductEnablementsByFilterQuery()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class GetProductEnablementsByFilterQueryResponse : ApplicationResponse<ProductEnablement>
	{
		public GetProductEnablementsByFilterQueryResponse(Tuple<int, int, WrapRequest<ProductEnablement>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public GetProductEnablementsByFilterQueryResponse(int statusCode, WrapRequest<ProductEnablement> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class GetProductEnablementsByFilterQueryHandler : ApplicationRequestHandler<ProductEnablement, GetProductEnablementsByFilterQuery, GetProductEnablementsByFilterQueryResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextReader Reader { get; set; }
		private ICurrentAdminContext AdminContext { get; set; }

		public GetProductEnablementsByFilterQueryHandler(
			ILoggerFactory logger,
			IMediator mediator,
			IStringLocalizer<ProductEnablement> localizer,
			IIdentityDbContextReader reader,
			ICurrentAdminContext adminContext)
		{
			Logger = logger; Localizer = localizer; Reader = reader; AdminContext = adminContext;
		}

		override public async Task<GetProductEnablementsByFilterQueryResponse> Handle(GetProductEnablementsByFilterQuery request, CancellationToken cancellationToken)
		{
			try
			{
				long resultCount = 1;
				var query = Reader.Query<ProductEnablement>().AsNoTracking().Where(x => !x.IsDeleted);

				if (!AdminContext.IsPlatformAdmin)
				{
					if (AdminContext.OrganizationId is not Guid orgId)
					{
						return new GetProductEnablementsByFilterQueryResponse((int)HttpStatusCode.OK, request, Array.Empty<ProductEnablement>(), Localizer["Successful operation!"], 0);
					}

					query = query.Where(x => x.OrganizationId == orgId);
				}

				var data = await query.FullSearch(request, out resultCount).ToListAsync(cancellationToken);
				return new GetProductEnablementsByFilterQueryResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], resultCount);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<GetProductEnablementsByFilterQueryHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new GetProductEnablementsByFilterQueryResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
