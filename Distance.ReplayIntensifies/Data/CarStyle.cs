using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Data
{
	public enum CarStyle
	{
		Ghost_Outline,
		Ghost_NoOutline,
		Networked_Outline,
		Networked_NoOutline,
		Replay_Outline,
		Replay_NoOutline,
	}

	public static class CarStyleExtensions
	{
		public static CarStyle[] GetSupportedMethodsList()
		{
			return new CarStyle[]
			{
				CarStyle.Ghost_Outline,
				CarStyle.Ghost_NoOutline,
				CarStyle.Networked_Outline,
				CarStyle.Networked_NoOutline,
				CarStyle.Replay_Outline,
				CarStyle.Replay_NoOutline,
			};
		}

		public static Dictionary<string, CarStyle> GetSettingsEntries()
		{
			return GetSupportedMethodsList().ToDictionary(m => m.GetSettingName());
		}

		public static string GetSettingName(this CarStyle carStyle)
		{
			string name = carStyle.ToString().Replace('_', ' ');
			if (carStyle.HasOutline())
			{
				name = name.Replace(" Outline", " (Outline)");
			}
			else
			{
				name = name.Replace(" NoOutline", " (no Outline)");
			}
			return name;
		}


		public static CarStyle ToCarStyle(CarLevelOfDetail.Type detailType, bool outline)
		{
			switch (detailType)
			{
			case CarLevelOfDetail.Type.Ghost:
				return (outline) ? CarStyle.Ghost_Outline : CarStyle.Ghost_NoOutline;

			case CarLevelOfDetail.Type.Networked:
				return (outline) ? CarStyle.Networked_Outline : CarStyle.Networked_NoOutline;

			case CarLevelOfDetail.Type.Replay:
				return (outline) ? CarStyle.Replay_Outline : CarStyle.Replay_NoOutline;

			default:
				goto case CarLevelOfDetail.Type.Ghost; // Fallback for invalid type
			}
		}

		public static bool HasOutline(this CarStyle carStyle)
		{
			switch (carStyle)
			{
			case CarStyle.Ghost_Outline:
			case CarStyle.Networked_Outline:
			case CarStyle.Replay_Outline:
				return true;

			case CarStyle.Ghost_NoOutline:
			case CarStyle.Networked_NoOutline:
			case CarStyle.Replay_NoOutline:
				return false;

			default:
				return false; // Fallback for invalid type
			}
		}

		public static CarLevelOfDetail.Type GetDetailType(this CarStyle carStyle)
		{
			switch (carStyle)
			{
			case CarStyle.Ghost_Outline:
			case CarStyle.Ghost_NoOutline:
				return CarLevelOfDetail.Type.Ghost;

			case CarStyle.Networked_Outline:
			case CarStyle.Networked_NoOutline:
				return CarLevelOfDetail.Type.Networked;

			case CarStyle.Replay_Outline:
			case CarStyle.Replay_NoOutline:
				return CarLevelOfDetail.Type.Replay;

			default:
				return CarLevelOfDetail.Type.Ghost; // Fallback for invalid type
			}
		}
	}
}
