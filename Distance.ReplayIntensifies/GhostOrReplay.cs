using System;

namespace Distance.ReplayIntensifies
{
	[Flags]
	public enum GhostOrReplay
	{
		Neither = 0,
		Ghosts  = 0x1,
		Replays = 0x2,
		Ghosts_and_Replays = Ghosts | Replays,
	}

	public static class GhostOrReplayExtensions
	{
		public static GhostOrReplay[] GetSupportedMethodsList()
		{
			return new GhostOrReplay[]
			{
				GhostOrReplay.Neither,
				GhostOrReplay.Ghosts,
				GhostOrReplay.Replays,
				GhostOrReplay.Ghosts_and_Replays,
			};
		}

		public static string GetSettingName(this GhostOrReplay ghostOrReplayMethod)
		{
			string name = ghostOrReplayMethod.ToString().Replace('_', ' ');
			name = name.Replace(" and ", " & ");
			return name;
		}
	}
}
