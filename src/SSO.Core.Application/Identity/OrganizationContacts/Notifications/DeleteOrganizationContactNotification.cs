using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationContacts.Notifications
{
	public sealed class DeleteOrganizationContactNotification : INotification
	{
		public OrganizationContact Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public DeleteOrganizationContactNotification(OrganizationContact payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class DeleteOrganizationContactNotificationHandler : INotificationHandler<DeleteOrganizationContactNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public DeleteOrganizationContactNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(DeleteOrganizationContactNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<DeleteOrganizationContactNotificationHandler>()
				.Log(LogLevel.Information, "OrganizationContact deleted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
