namespace SSO.Infrastructures.Services
{
	public sealed class SmtpMailSettings
	{
		public const string SectionName = "Sso:Mail";

		public bool Enabled { get; set; }

		/// <summary>Gmail submission host. Override only for a Google Workspace SMTP relay.</summary>
		public string Host { get; set; } = "smtp.gmail.com";

		/// <summary>587 with STARTTLS, or 465 with implicit TLS.</summary>
		public int Port { get; set; } = 587;

		public bool UseStartTls { get; set; } = true;

		public string UserName { get; set; } = string.Empty;

		/// <summary>Google App Password. Do not commit a real value.</summary>
		public string Password { get; set; } = string.Empty;

		public string FromAddress { get; set; } = string.Empty;

		public string FromName { get; set; } = "atNodus";

		public int TimeoutSeconds { get; set; } = 30;
	}
}
