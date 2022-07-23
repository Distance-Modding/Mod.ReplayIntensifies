using HarmonyLib;
using System;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to override the level of detail range that cars can render at.
	/// </summary>
	/// <remarks>
	/// Required For: Min/Max Car Level of Detail.
	/// </remarks>
	[HarmonyPatch(typeof(CarLevelOfDetail), nameof(CarLevelOfDetail.SetLevelOfDetail))]
	internal static class CarLevelOfDetail__SetLevelOfDetail
	{
		[HarmonyPrefix]
		internal static bool Prefix(CarLevelOfDetail __instance, CarLevelOfDetail.Level newLevel)
		{
			// Avoid clamping if the max is unchanged (InFocusFP), just in-case other mods mess with this...
			// NOTE: Highest LOD is lowest-value enum, so compare using less than.
			CarLevelOfDetail.Level maxLevel = Mod.Instance.Config.MaxLevelOfDetailCached;

			// NOTE: Replay-type cars will NOT RENDER A SOLID BODY at Speck, so force the minimum to VeryFar.
			//       Interestingly, networked cars WILL render a solid body at Speck.
			if (__instance.type_ == CarLevelOfDetail.Type.Replay && maxLevel > CarLevelOfDetail.Level.VeryFar)
			{
				maxLevel = CarLevelOfDetail.Level.VeryFar;
			}
			if (newLevel < maxLevel && maxLevel != CarLevelOfDetail.Level.InFocusFP)
			{
				newLevel = maxLevel;
			}
			else
			{
				// Avoid clamping if the min is unchanged (Speck), just in-case other mods mess with this...
				// MaxLevel has priority over MinLevel (when the range is invalid)
				CarLevelOfDetail.Level minLevel = Mod.Instance.Config.MinLevelOfDetailCached;
				if (newLevel > minLevel && minLevel != CarLevelOfDetail.Level.Speck)
				{
					newLevel = minLevel;
				}
			}

			if (__instance.level_ != newLevel)
			{
				__instance.LevelOfDetailChange(__instance.level_, newLevel);
				__instance.levelSwitchTimer_ = 0f;
			}

			return false; // Don't run original method
		}
	}
}
