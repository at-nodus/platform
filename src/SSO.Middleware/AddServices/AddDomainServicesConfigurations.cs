using System;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;
using SSO.Core.Domain.Identity._Context.Interfaces.Services;
using SSO.Infrastructures.Services;
using SSO.Infrastructures.Services.Identity;
using SSO.Middleware.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SSO.Middleware.AddServices
{
	public static class AddDomainServicesConfigurations
	{
		public static IServiceCollection AddDomainServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddMail(configuration);
			services.AddScoped<IAuthAuditService, AuthAuditService>();
			services.AddScoped<IUserSessionService, UserSessionService>();
			services.AddHttpClient("sso-webhooks", client =>
			{
				client.Timeout = TimeSpan.FromSeconds(10);
			});
			services.AddHostedService<WebhookOutboxSenderHostedService>();

			return services;
		}

		private static void AddMail(this IServiceCollection services, IConfiguration configuration)
		{
			var settings = configuration.GetSection(SmtpMailSettings.SectionName).Get<SmtpMailSettings>()
				?? new SmtpMailSettings();

			if (!settings.Enabled)
			{
				services.AddSingleton<IMailService, MailService>();
				return;
			}

			if (string.IsNullOrWhiteSpace(settings.Host))
			{
				throw new InvalidOperationException("Sso:Mail is Enabled but Host is missing.");
			}

			if (settings.Port <= 0)
			{
				throw new InvalidOperationException("Sso:Mail is Enabled but Port is invalid.");
			}

			if (string.IsNullOrWhiteSpace(settings.UserName) || string.IsNullOrWhiteSpace(settings.Password))
			{
				throw new InvalidOperationException(
					"Sso:Mail is Enabled but UserName/Password are missing. Use a Google App Password via environment variables (Sso__Mail__UserName / Sso__Mail__Password).");
			}

			if (string.IsNullOrWhiteSpace(settings.FromAddress))
			{
				settings.FromAddress = settings.UserName;
			}

			services.AddSingleton(settings);
			services.AddSingleton<IMailService, SmtpMailService>();
		}
	}
}
