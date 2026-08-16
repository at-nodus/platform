using Newtonsoft.Json;

namespace SSO.Core.Application.Identity._Shared
{
	internal static class NotificationPayloadSerializer
	{
		private static readonly JsonSerializerSettings Settings = new()
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore
		};

		public static string Serialize(object payload)
			=> JsonConvert.SerializeObject(payload, Settings);
	}
}
