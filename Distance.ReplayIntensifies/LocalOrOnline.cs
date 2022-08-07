using System;

namespace Distance.ReplayIntensifies
{
	[Flags]
	public enum LocalOrOnline
	{
		Neither = 0,
		Local   = 0x1,
		Online  = 0x2,
		Local_and_Online = Local | Online,
	}

	public static class LocalOrOnlineExtensions
	{
		public static LocalOrOnline[] GetSupportedMethodsList()
		{
			return new LocalOrOnline[]
			{
				LocalOrOnline.Neither,
				LocalOrOnline.Local,
				LocalOrOnline.Online,
				LocalOrOnline.Local_and_Online,
			};
		}

		public static string GetSettingName(this LocalOrOnline localOrOnlineMethod)
		{
			string name = localOrOnlineMethod.ToString().Replace('_', ' ');
			name = name.Replace(" and ", " & ");
			return name;
		}
	}
}
