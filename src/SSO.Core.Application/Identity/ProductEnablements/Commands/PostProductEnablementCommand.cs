using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
using ModelWrapper.Extensions.Post;
using SSO.Core.Application.Identity.ProductEnablements.Notifications;
using SSO.Core.Domain.Identity._Context.Interfaces.Infrastructures.Data;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using SSO.Core.Domain.Identity.ProductEnablements.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ProductEnablements.Commands
{
	public sealed class PostProductEnablementCommand : ApplicationRequest<ProductEnablement, PostProductEnablementCommandResponse>
	{
		public PostProductEnablementCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
		}
	}

	public sealed class PostProductEnablementCommandResponse : ApplicationResponse<ProductEnablement>
	{
		public PostProductEnablementCommandResponse(Tuple<int, int, WrapRequest<ProductEnablement>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public PostProductEnablementCommandResponse(int statusCode, WrapRequest<ProductEnablement> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class PostProductEnablementCommandHandler : ApplicationRequestHandler<ProductEnablement, PostProductEnablementCommand, PostProductEnablementCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public PostProductEnablementCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<ProductEnablement> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<PostProductEnablementCommandResponse> Handle(PostProductEnablementCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var data = request.Post();
				data.MarkCreated();
				await Mediator.Send(new CreateProductEnablementServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				data.Organization = null;
				data.Product = null;
				await Mediator.Publish(new PostProductEnablementNotification(data));
				return new PostProductEnablementCommandResponse((int)HttpStatusCode.Created, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<PostProductEnablementCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new PostProductEnablementCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
