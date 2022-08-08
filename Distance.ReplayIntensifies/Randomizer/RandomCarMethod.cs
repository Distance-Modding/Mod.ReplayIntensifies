using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Randomizer
{
	public enum RandomCarMethod
	{
		Off,
		Car_Types,
		Car_Types_Cycle,
		Car_Types_Ordered,
	}

	public static class RandomCarMethodExtensions
	{
		public static RandomCarMethod[] GetSupportedMethodsList()
		{
			return new RandomCarMethod[]
			{
				RandomCarMethod.Off,
				RandomCarMethod.Car_Types,
				RandomCarMethod.Car_Types_Cycle,
				//RandomCarMethod.Car_Types_Ordered,
			};
		}

		public static Dictionary<string, RandomCarMethod> GetSettingsEntries()
		{
			return GetSupportedMethodsList().ToDictionary(m => m.GetSettingName());
		}

		public static string GetSettingName(this RandomCarMethod carMethod)
		{
			string name = carMethod.ToString().Replace('_', ' ');
			switch (carMethod)
			{
			case RandomCarMethod.Car_Types_Cycle:
				name = name.Replace("Cycle", "(Cycle)");
				break;

			case RandomCarMethod.Car_Types_Ordered:
				name = name.Replace("Ordered", "(Ordered)");
				break;
			}
			return name;
		}

		public static bool IsCarTypes(this RandomCarMethod carMethod)
		{
			switch (carMethod)
			{
			case RandomCarMethod.Car_Types:
			case RandomCarMethod.Car_Types_Cycle:
			case RandomCarMethod.Car_Types_Ordered:
				return true;

			default:
				return false;
			}
		}

		public static bool IsAvoidDuplicates(this RandomCarMethod carMethod)
		{
			switch (carMethod)
			{
			case RandomCarMethod.Car_Types_Cycle:
			case RandomCarMethod.Car_Types_Ordered:
				return true;

			default:
				return false;
			}
		}
	}
}
