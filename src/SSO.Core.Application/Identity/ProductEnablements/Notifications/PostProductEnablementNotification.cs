using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.ProductEnablements.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.ProductEnablements.Notifications
{
	public sealed class PostProductEnablementNotification : INotification
	{
		public ProductEnablement Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostProductEnablementNotification(ProductEnablement payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostProductEnablementNotificationHandler : INotificationHandler<PostProductEnablementNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostProductEnablementNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostProductEnablementNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostProductEnablementNotificationHandler>()
				.Log(LogLevel.Information, "ProductEnablement posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
