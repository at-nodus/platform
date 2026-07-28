using BAYSOFT.Abstractions.Core.Application;
using BAYSOFT.Abstractions.Core.Domain.Exceptions;
using BAYSOFT.Abstractions.Crosscutting.Helpers;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ModelWrapper;
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
	public sealed class DeleteProductEnablementCommand : ApplicationRequest<ProductEnablement, DeleteProductEnablementCommandResponse>
	{
		public DeleteProductEnablementCommand()
		{
			ConfigKeys(x => x.Id);
			ConfigSuppressedProperties(x => x.Id);
			Validator.RuleFor(x => x.Id).NotEmpty().WithMessage("{0} is required!");
		}
	}

	public sealed class DeleteProductEnablementCommandResponse : ApplicationResponse<ProductEnablement>
	{
		public DeleteProductEnablementCommandResponse(Tuple<int, int, WrapRequest<ProductEnablement>, Dictionary<string, object>, Dictionary<string, object>, string, long?> tuple) : base(tuple) { }
		public DeleteProductEnablementCommandResponse(int statusCode, WrapRequest<ProductEnablement> request, object data, string message = "Successful operation!", long? resultCount = null) : base(statusCode, request, data, message, resultCount) { }
	}

	public sealed class DeleteProductEnablementCommandHandler : ApplicationRequestHandler<ProductEnablement, DeleteProductEnablementCommand, DeleteProductEnablementCommandResponse>
	{
		private ILoggerFactory Logger { get; set; }
		private IMediator Mediator { get; set; }
		private IStringLocalizer Localizer { get; set; }
		private IIdentityDbContextWriter Writer { get; set; }

		public DeleteProductEnablementCommandHandler(ILoggerFactory logger, IMediator mediator, IStringLocalizer<ProductEnablement> localizer, IIdentityDbContextWriter writer)
		{
			Logger = logger; Mediator = mediator; Localizer = localizer; Writer = writer;
		}

		override public async Task<DeleteProductEnablementCommandResponse> Handle(DeleteProductEnablementCommand request, CancellationToken cancellationToken)
		{
			try
			{
				request.IsValid(Localizer, true);
				var id = request.Project(x => x.Id);
				var data = await Writer.Query<ProductEnablement>().SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
				if (data == null) throw new EntityNotFoundException<ProductEnablement>(Localizer);
				await Mediator.Send(new DeleteProductEnablementServiceRequest(data));
				await Writer.CommitAsync(cancellationToken);
				data.Organization = null;
				data.Product = null;
				await Mediator.Publish(new DeleteProductEnablementNotification(data));
				return new DeleteProductEnablementCommandResponse((int)HttpStatusCode.OK, request, data, Localizer["Successful operation!"], 1);
			}
			catch (Exception exception)
			{
				Logger.CreateLogger<DeleteProductEnablementCommandHandler>().Log(LogLevel.Error, exception, exception.Message);
				return new DeleteProductEnablementCommandResponse(ExceptionResponseHelper.CreateTuple(Localizer, request, exception));
			}
		}
	}
}
