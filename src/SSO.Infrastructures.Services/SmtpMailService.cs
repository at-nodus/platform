using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;

namespace SSO.Infrastructures.Services
{
	/// <summary>Sends mail through Google SMTP (smtp.gmail.com) via MailKit.</summary>
	public sealed class SmtpMailService : IMailService
	{
		private readonly ILogger<SmtpMailService> _logger;
		private readonly SmtpMailSettings _settings;

		public SmtpMailService(ILogger<SmtpMailService> logger, SmtpMailSettings settings)
		{
			_logger = logger;
			_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

		public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(to))
			{
				throw new ArgumentException("Recipient is required.", nameof(to));
			}

			if (string.IsNullOrWhiteSpace(_settings.UserName) || string.IsNullOrWhiteSpace(_settings.Password))
			{
				throw new InvalidOperationException(
					"SMTP credentials are not configured (Sso:Mail:UserName / Sso:Mail:Password).");
			}

			var fromAddress = string.IsNullOrWhiteSpace(_settings.FromAddress)
				? _settings.UserName
				: _settings.FromAddress;

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(_settings.FromName ?? string.Empty, fromAddress));
			message.To.Add(MailboxAddress.Parse(to.Trim()));
			message.Subject = subject ?? string.Empty;
			message.Body = new TextPart("plain") { Text = body ?? string.Empty };

			using var client = new SmtpClient
			{
				Timeout = _settings.TimeoutSeconds > 0 ? _settings.TimeoutSeconds * 1000 : 30000
			};

			var socketOptions = _settings.UseStartTls
				? SecureSocketOptions.StartTls
				: SecureSocketOptions.SslOnConnect;

			try
			{
				await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions, cancellationToken);
				await client.AuthenticateAsync(_settings.UserName, _settings.Password, cancellationToken);
				await client.SendAsync(message, cancellationToken);
				await client.DisconnectAsync(true, cancellationToken);
			}
			catch (Exception exception) when (exception is not OperationCanceledException)
			{
				_logger.LogError(
					exception,
					"SMTP send failed to {To} via {Host}:{Port}",
					to,
					_settings.Host,
					_settings.Port);
				throw;
			}

			_logger.LogInformation("SMTP mail sent to {To} | {Subject}", to, subject);
		}
	}
}
