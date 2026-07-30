using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.OrganizationContacts.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using SSO.Core.Domain.Identity.OrganizationContacts.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationContacts.Commands
{
	public sealed class PostOrganizationContactCommand : ApplicationRequest<OrganizationContact, PostOrganizationContactCommandResponse>
	{
		public PostOrganizationContactCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostOrganizationContactCommandResponse : ApplicationResponse<OrganizationContact>
	{
		public PostOrganizationContactCommandResponse(Tuple<int, int, WrapRequest<OrganizationContact>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostOrganizationContactCommandResponse(int statusCode, WrapRequest<OrganizationContact> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostOrganizationContactCommandHandler : ApplicationRequestHandler<OrganizationContact, PostOrganizationContactCommand, PostOrganizationContactCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostOrganizationContactCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<OrganizationContact> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostOrganizationContactCommandResponse> Handle(PostOrganizationContactCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreateOrganizationContactServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				data.Organization = null;
				await Mediator.Publish(new PostOrganizationContactNotification(data));
				return new PostOrganizationContactCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostOrganizationContactCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostOrganizationContactCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
