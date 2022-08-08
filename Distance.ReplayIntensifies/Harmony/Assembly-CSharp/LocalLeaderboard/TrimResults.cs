using Distance.ReplayIntensifies.Data;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to extend limit for saved local replays, by preventing replays from being removed from the leaderboards.
	/// </summary>
	/// <remarks>
	/// Required For: Local Replay Trimming, Max Saved Local Replays (part 2/2).
	/// </remarks>
	[HarmonyPatch(typeof(LocalLeaderboard), nameof(LocalLeaderboard.TrimResults))]
	internal static class LocalLeaderboard__TrimResults
	{
		[HarmonyPrefix]
		internal static bool Prefix(LocalLeaderboard __instance)
		{
			int count = __instance.results_.Count;

			int limit = Mod.GetMaxSavedLocalReplays();
			var trimming = Mod.Instance.Config.LocalReplayTrimming;

			// Remove if trimming is fully enabled and we're over the count limit.
			if (trimming == LocalLeaderboardTrimming.Always && count > limit)
			{
				for (int i = limit; i < count; i++)
				{
					FileEx.DeleteIfExists(CarReplayData.GetReplayFilePath(__instance, __instance.results_[i].ReplayGuid_, false));
				}
				__instance.results_.RemoveRange(limit, count - limit);
			}

			return false; // Don't run original method.
		}

		// void TrimResults()
		/*[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			// Changes the max count comparison operands (20) to the new maximum specified by the mod.
			//if (count > 20)
			//{
			//	for (int i = 20; i < count; i++)
			//	{
			//		FileEx.DeleteIfExists(CarReplayData.GetReplayFilePath(this, this.results_[i].ReplayGuid_, false));
			//	}
			//	this.results_.RemoveRange(20, count - 20);
			//}

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 0; i < codes.Count; i++)
			{
				// The only instance of 20 appearing as an operand in this function is for the max comparison.
				if (codes[i].opcode == OpCodes.Ldc_I4_S && codes[i].operand.ToString() == "20")
				{
					Mod.Instance.Logger.Info($"ldc.i4.s 20 @ {i}");

					// Replace: ldc.i4.s 20
					// With:    call Mod.GetMaxSavedLocalReplays
					codes[i].opcode = OpCodes.Call;
					codes[i].operand = typeof(Mod).GetMethod(nameof(Mod.GetMaxSavedLocalReplays));

					// This instruction appears 4 times, so no breaking after our first find.
				}
			}
			return codes.AsEnumerable();
		}*/
	}
}
