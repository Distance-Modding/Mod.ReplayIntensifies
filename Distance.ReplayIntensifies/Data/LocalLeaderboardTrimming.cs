using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Data
{
	public enum LocalLeaderboardTrimming
	{
		Never,   // Never trim replays
		Current, // Only trim current replay (if past limit)
		Always,  // Always trim replays
	}

	public static class LocalLeaderboardTrimmingExtensions
	{
		public static LocalLeaderboardTrimming[] GetSupportedMethodsList()
		{
			return new LocalLeaderboardTrimming[]
			{
				LocalLeaderboardTrimming.Never,
				LocalLeaderboardTrimming.Current,
				LocalLeaderboardTrimming.Always,
			};
		}

		public static Dictionary<string, LocalLeaderboardTrimming> GetSettingsEntries()
		{
			return GetSupportedMethodsList().ToDictionary(m => m.GetSettingName());
		}

		public static string GetSettingName(this LocalLeaderboardTrimming localLeaderboardTrimmingMethod)
		{
			string name = localLeaderboardTrimmingMethod.ToString().Replace('_', ' ');
			switch (localLeaderboardTrimmingMethod)
			{
			case LocalLeaderboardTrimming.Current:
				name = "Current Run Only";
				break;
			}
			return name;
		}
	}
}
