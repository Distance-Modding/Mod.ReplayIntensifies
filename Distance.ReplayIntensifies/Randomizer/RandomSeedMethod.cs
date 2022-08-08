using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Randomizer
{
	public enum RandomSeedMethod
	{
		Always_Random,  // Environment.Ticks + increment.
		By_Replay,      // Crc.Hash32 of CarReplayData.StateBuffer_ + CarReplayData.EventBuffer_.
		By_Placement,   // Placement between current replays (1-indexed). 0 for DidNotFinish, excludes your replay for the last run.
	}

	public static class RandomSeedMethodExtensions
	{
		public static RandomSeedMethod[] GetSupportedMethodsList()
		{
			return new RandomSeedMethod[]
			{
				RandomSeedMethod.Always_Random,
				RandomSeedMethod.By_Replay,
				RandomSeedMethod.By_Placement,
			};
		}

		public static Dictionary<string, RandomSeedMethod> GetSettingsEntries()
		{
			return GetSupportedMethodsList().ToDictionary(m => m.GetSettingName());
		}

		public static string GetSettingName(this RandomSeedMethod seedMethod)
		{
			string name = seedMethod.ToString().Replace('_', ' ');
			return name;
		}

		public static bool IsFixed(this RandomSeedMethod seedMethod)
		{
			switch (seedMethod)
			{
			case RandomSeedMethod.By_Replay:
			case RandomSeedMethod.By_Placement:
				return true;

			default:
				return false;
			}
		}
	}
}
