using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.Core.Domain.Identity.OrganizationContacts.Entity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSO.Core.Application.Identity.OrganizationContacts.Notifications
{
	public sealed class PutOrganizationContactNotification : INotification
	{
		public OrganizationContact Payload { get; set; }
		public DateTime CreatedAt { get; set; }
		public PutOrganizationContactNotification(OrganizationContact payload)
		{
			Payload = payload;
			CreatedAt = DateTime.UtcNow;
		}
	}

	public sealed class PutOrganizationContactNotificationHandler : INotificationHandler<PutOrganizationContactNotification>
	{
		private ILoggerFactory Logger { get; set; }
		public PutOrganizationContactNotificationHandler(ILoggerFactory logger) { Logger = logger; }
		public Task Handle(PutOrganizationContactNotification notification, CancellationToken cancellationToken)
		{
			Logger.CreateLogger<PutOrganizationContactNotificationHandler>()
				.Log(LogLevel.Information, "OrganizationContact putted! Payload: {Payload}", JsonConvert.SerializeObject(notification.Payload));
			return Task.CompletedTask;
		}
	}
}
