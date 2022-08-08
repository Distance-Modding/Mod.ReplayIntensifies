using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Randomizer
{
	public enum RandomColorMethod
	{
		Off,
		Color_Presets,
		Color_Presets_Cycle,
		Color_Presets_Ordered,
		HSV,
		RGB,
		Default_Colors,
	}

	public static class RandomColorMethodExtensions
	{
		public static RandomColorMethod[] GetSupportedMethodsList()
		{
			return new RandomColorMethod[]
			{
				RandomColorMethod.Off,
				RandomColorMethod.Color_Presets,
				RandomColorMethod.Color_Presets_Cycle,
				//RandomColorMethod.Color_Presets_Ordered,
				RandomColorMethod.HSV,
				RandomColorMethod.RGB,
				RandomColorMethod.Default_Colors,
			};
		}

		public static Dictionary<string, RandomColorMethod> GetSettingsEntries()
		{
			return GetSupportedMethodsList().ToDictionary(m => m.GetSettingName());
		}

		public static string GetSettingName(this RandomColorMethod colorMethod)
		{
			string name = colorMethod.ToString().Replace('_', ' ');
			switch (colorMethod)
			{
			case RandomColorMethod.Color_Presets_Cycle:
				name = name.Replace("Cycle", "(Cycle)");
				break;

			case RandomColorMethod.Color_Presets_Ordered:
				name = name.Replace("Ordered", "(Ordered)");
				break;
			}
			return name;
		}

		public static bool IsColorPresets(this RandomColorMethod colorMethod)
		{
			switch (colorMethod)
			{
			case RandomColorMethod.Color_Presets:
			case RandomColorMethod.Color_Presets_Cycle:
			case RandomColorMethod.Color_Presets_Ordered:
				return true;

			default:
				return false;
			}
		}

		public static bool IsAvoidDuplicates(this RandomColorMethod colorMethod)
		{
			switch (colorMethod)
			{
			case RandomColorMethod.Color_Presets_Cycle:
			case RandomColorMethod.Color_Presets_Ordered:
				return true;

			default:
				return false;
			}
		}
	}
}
