using System;
using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Data
{
	[Flags]
	public enum LocalOrOnline
	{
		Neither         = 0,
		Local_Replays   = 0x1,
		Online_Replays  = 0x2,
		Local_And_Online_Replays = Local_Replays | Online_Replays,
	}

	public static class LocalOrOnlineExtensions
	{
		public static LocalOrOnline[] GetSupportedMethodsList()
		{
			return new LocalOrOnline[]
			{
				LocalOrOnline.Neither,
				LocalOrOnline.Local_Replays,
				LocalOrOnline.Online_Replays,
				LocalOrOnline.Local_And_Online_Replays,
			};
		}

		public static Dictionary<string, LocalOrOnline> GetSettingsEntries()
		{
			return GetSupportedMethodsList().ToDictionary(m => m.GetSettingName());
		}

		public static string GetSettingName(this LocalOrOnline localOrOnlineMethod)
		{
			string name = localOrOnlineMethod.ToString().Replace('_', ' ');
			name = name.Replace(" And ", " & ");
			return name;
		}
	}
}
