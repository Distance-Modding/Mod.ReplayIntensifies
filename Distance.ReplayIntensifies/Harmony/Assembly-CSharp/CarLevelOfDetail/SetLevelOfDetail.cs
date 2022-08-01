using HarmonyLib;

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
			CarLevelOfDetail.Level origNewLevel = newLevel;

			// Avoid clamping if the max is unchanged (InFocusFP), just in-case other mods mess with this...
			// NOTE: Highest LOD is lowest-value enum, so compare using less than.
			CarLevelOfDetail.Level maxLevel = Mod.Instance.Config.MaxLevelOfDetailCached;
			CarLevelOfDetail.Level minLevel = Mod.Instance.Config.MinLevelOfDetailCached;

			// NOTE: Replay-type cars will NOT RENDER A SOLID BODY at Speck, so force the minimum to VeryFar.
			//       Interestingly, networked cars WILL render a solid body at Speck.
			if (__instance.type_ == CarLevelOfDetail.Type.Replay && maxLevel > CarLevelOfDetail.Level.VeryFar)
			{
				maxLevel = CarLevelOfDetail.Level.VeryFar; // Max LOD cannot be lower than Very Far, when in replay mode.
			}

			// Exclude In-Focus/In-Focus FP from max LOD checks (these are only used for replay cars with camera focus).
			if (newLevel >= CarLevelOfDetail.Level.Near && newLevel < maxLevel /*&& maxLevel != CarLevelOfDetail.Level.InFocusFP*/)
			{
				newLevel = maxLevel;
			}
			else
			{
				// Avoid clamping if the min is unchanged (Speck), just in-case other mods mess with this...
				// MaxLevel has priority over MinLevel (when the range is invalid)
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

			// The NitronicCarController houses the most CPU-intensive processes for a car's LOD.
			// These processes are so intensive, that they can actually mess up warp anchor physics when used with too many cars.
			//  (i.e. try playing Breakout with ~40-50 ghosts, >= Medium min LOD, and replay/networked car styles)
			CarLevelOfDetail.Level controllerLevel = (__instance.type_ == CarLevelOfDetail.Type.Ghost) ? CarLevelOfDetail.Level.InFocus : CarLevelOfDetail.Level.Medium;

			// To fix this, we have a threshold to disable the NitronicCarController regardless of what LOD it'd be enabled at.
			if (minLevel <= controllerLevel && origNewLevel > CarLevelOfDetail.Level.InFocus)
			{
				// Override the NitronicCarController enabled state if our min LOD forces it to always be enabled.
				// But only override the NitronicCarController enabled state when the car isn't *actually* in-focus.
				__instance.nitronicCarController_.enabled = (PlayerDataReplay.ReplayPlayers_.Count <= 20);
			}
			else
			{
				// Default enabled state, though this could be changed by modifying `controllerLevel`.
				//  (e.g. it may be beneficial to place the controllerLevel at Near for non-ghost detail types).
				__instance.nitronicCarController_.enabled = (newLevel <= controllerLevel);// && PlayerDataReplay.ReplayPlayers_.Count <= 20;
			}

			return false; // Don't run original method
		}
	}
}
