using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationContacts.Notifications
{
	public sealed class PostOrganizationContactNotification : INotification
	{
		public OrganizationContact Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PostOrganizationContactNotification(OrganizationContact payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PostOrganizationContactNotificationHandler : INotificationHandler<PostOrganizationContactNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PostOrganizationContactNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PostOrganizationContactNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PostOrganizationContactNotificationHandler>()
				.Log(LogLevel.Information, "OrganizationContact posted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
