using System;
using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Data
{
	[Flags]
	public enum GhostOrReplay
	{
		Neither     = 0,
		Ghost_Mode  = 0x1,
		Replay_Mode = 0x2,
		Ghost_And_Replay_Mode = Ghost_Mode | Replay_Mode,
	}

	public static class GhostOrReplayExtensions
	{
		public static GhostOrReplay[] GetSupportedMethodsList()
		{
			return new GhostOrReplay[]
			{
				GhostOrReplay.Neither,
				GhostOrReplay.Ghost_Mode,
				GhostOrReplay.Replay_Mode,
				GhostOrReplay.Ghost_And_Replay_Mode,
			};
		}

		public static Dictionary<string, GhostOrReplay> GetSettingsEntries()
		{
			return GetSupportedMethodsList().ToDictionary(m => m.GetSettingName());
		}

		public static string GetSettingName(this GhostOrReplay ghostOrReplayMethod)
		{
			string name = ghostOrReplayMethod.ToString().Replace('_', ' ');
			name = name.Replace(" And ", " & ");
			return name;
		}

		public static bool HasGhostOrReplay(this GhostOrReplay ghostOrReplayMethod, bool isGhost)
		{
			if (isGhost)
			{
				return ghostOrReplayMethod.HasFlag(GhostOrReplay.Ghost_Mode);
			}
			else
			{
				return ghostOrReplayMethod.HasFlag(GhostOrReplay.Replay_Mode);
			}
		}
	}
}
