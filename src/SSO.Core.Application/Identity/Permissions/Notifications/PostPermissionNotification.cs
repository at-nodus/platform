using MediatR;
using Microsoft.Extensions.Logging;
using SSO.Core.Application.Identity._Shared;
using SSO.Core.Domain.Identity.Permissions.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.Permissions.Notifications
{
	public sealed class PostPermissionNotification : INotification
	{
		public Permission Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostPermissionNotification(Permission payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostPermissionNotificationHandler : INotificationHandler<PostPermissionNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostPermissionNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostPermissionNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostPermissionNotificationHandler>()
				.Log(LogLevel.Information, "Permission posted! Payload: {Payload}", NotificationPayloadSerializer.Serialize(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
