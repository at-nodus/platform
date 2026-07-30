using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
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
	public sealed class DeleteOrganizationContactCommand : ApplicationRequest<OrganizationContact, DeleteOrganizationContactCommandResponse>
	{
		public DeleteOrganizationContactCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class DeleteOrganizationContactCommandResponse : ApplicationResponse<OrganizationContact>
	{
		public DeleteOrganizationContactCommandResponse(Tuple<int, int, WrapRequest<OrganizationContact>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public DeleteOrganizationContactCommandResponse(int statusCode, WrapRequest<OrganizationContact> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class DeleteOrganizationContactCommandHandler : ApplicationRequestHandler<OrganizationContact, DeleteOrganizationContactCommand, DeleteOrganizationContactCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public DeleteOrganizationContactCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<OrganizationContact> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<DeleteOrganizationContactCommandResponse> Handle(DeleteOrganizationContactCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<OrganizationContact>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<OrganizationContact>(Localizer);
				await Mediator.Send(new DeleteOrganizationContactServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				data.Organization = null;
				await Mediator.Publish(new DeleteOrganizationContactNotification(data));
				return new DeleteOrganizationContactCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<DeleteOrganizationContactCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new DeleteOrganizationContactCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
