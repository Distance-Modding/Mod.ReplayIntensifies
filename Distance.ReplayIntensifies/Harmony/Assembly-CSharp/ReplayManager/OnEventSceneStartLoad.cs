using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to handle starting replay mode in a paused state
	/// </summary>
	/// <remarks>
	/// Required For: Start Paused in Replay Mode.
	/// </remarks>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.OnEventSceneStartLoad))]
	internal static class ReplayManager__OnEventSceneStartLoad
	{
		[HarmonyPostfix]
		internal static void Postfix(ReplayManager __instance)
		{
			if (__instance.IsReplayMode_ && Mod.Instance.Config.ReplayModePauseAtStart)
			{
				__instance.TogglePause();
			}
		}

		/*[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//this.UpdateAudioSpeed(false);
			// -to-
			//HandleStartReplayPaused_(this);

			for (int i = 1; i < codes.Count; i++)
			{
				if ((codes[i - 1].opcode == OpCodes.Ldc_I4_0) &&
					(codes[i    ].opcode == OpCodes.Call && ((MethodInfo)codes[i    ].operand).Name == "UpdateAudioSpeed"))
				{
					Mod.Instance.Logger.Info($"call UpdateAudioSpeed @ {i}");

					// Replace: ldc.i4.0 (false)
					// Replace: call UpdateAudioSpeed
					// With:    call HandleStartReplayPaused_
					codes.RemoveRange(i - 1, 2);
					codes.InsertRange(i - 1, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Call, typeof(ReplayManager__OnEventSceneStartLoad).GetMethod(nameof(HandleStartReplayPaused_))),
					});

					break;
				}
			}

			return codes.AsEnumerable();
		}

		#region Helpers

		public static void HandleStartReplayPaused_(ReplayManager replayManager)
		{
			if (replayManager.IsReplayMode_ && Mod.Instance.Config.ReplayModeStartPaused)
			{
				replayManager.TogglePause();
			}
			else
			{
				replayManager.UpdateAudioSpeed(false);
			}
		}

		#endregion*/
	}
}
