using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ProductEnablements.Notifications
{
	public sealed class DeleteProductEnablementNotification : INotification
	{
		public ProductEnablement Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteProductEnablementNotification(ProductEnablement payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteProductEnablementNotificationHandler : INotificationHandler<DeleteProductEnablementNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteProductEnablementNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteProductEnablementNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteProductEnablementNotificationHandler>()
				.Log(LogLevel.Information, "ProductEnablement deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
