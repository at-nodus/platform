using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SSO.Core.Domain.Interfaces.Infrastructures.Services;
using SSO.Infrastructures.Services;
using SSO.Middleware.AddServices;

namespace SSO.Tests.UnitTests.Infrastructures.Services
{
	[TestClass]
	public class SmtpMailServiceScenarios
	{
		[TestMethod]
		public async Task SendAsync_Without_Credentials_Throws_Before_Connect()
		{
			var service = new SmtpMailService(
				NullLogger<SmtpMailService>.Instance,
				new SmtpMailSettings
				{
					Enabled = true,
					Host = "smtp.gmail.com",
					Port = 587,
					UserName = string.Empty,
					Password = string.Empty
				});

			await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
				service.SendAsync("user@test.local", "subject", "body"));
		}

		[TestMethod]
		public async Task SendAsync_Without_Recipient_Throws()
		{
			var service = new SmtpMailService(
				NullLogger<SmtpMailService>.Instance,
				new SmtpMailSettings
				{
					UserName = "sender@gmail.com",
					Password = "app-password"
				});

			await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
				service.SendAsync("  ", "subject", "body"));
		}

		[TestMethod]
		public void AddDomainServices_When_Mail_Disabled_Registers_Logger_MailService()
		{
			var services = new ServiceCollection();
			services.AddLogging();
			services.AddDomainServices(BuildConfig(enabled: false));

			using var provider = services.BuildServiceProvider();
			var mail = provider.GetRequiredService<IMailService>();

			Assert.IsInstanceOfType(mail, typeof(MailService));
		}

		[TestMethod]
		public void AddDomainServices_When_Mail_Enabled_Registers_SmtpMailService()
		{
			var services = new ServiceCollection();
			services.AddLogging();
			services.AddDomainServices(BuildConfig(enabled: true));

			using var provider = services.BuildServiceProvider();
			var mail = provider.GetRequiredService<IMailService>();

			Assert.IsInstanceOfType(mail, typeof(SmtpMailService));
		}

		[TestMethod]
		public void AddDomainServices_When_Mail_Enabled_Without_Password_Throws()
		{
			var services = new ServiceCollection();
			var config = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["Sso:Mail:Enabled"] = "true",
					["Sso:Mail:UserName"] = "sender@gmail.com",
					["Sso:Mail:Password"] = ""
				})
				.Build();

			Assert.ThrowsExactly<InvalidOperationException>(() => services.AddDomainServices(config));
		}

		private static IConfiguration BuildConfig(bool enabled)
		{
			return new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					["Sso:Mail:Enabled"] = enabled ? "true" : "false",
					["Sso:Mail:Host"] = "smtp.gmail.com",
					["Sso:Mail:Port"] = "587",
					["Sso:Mail:UseStartTls"] = "true",
					["Sso:Mail:UserName"] = "sender@gmail.com",
					["Sso:Mail:Password"] = "app-password",
					["Sso:Mail:FromAddress"] = "sender@gmail.com",
					["Sso:Mail:FromName"] = "atNodus"
				})
				.Build();
		}
	}
}
