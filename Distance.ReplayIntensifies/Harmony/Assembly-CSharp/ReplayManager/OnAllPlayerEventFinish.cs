using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to mark replays that were just created from the player's last run (so that we don't randomize them).
	/// </summary>
	/// <remarks>
	/// Required For: Randomized Cars.
	/// </remarks>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.OnAllPlayerEventFinish))]
	internal static class ReplayManager__OnAllPlayerEventFinish
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//this.AddReplayToGroup(component.FinishAndGetReplayData(data.finishData_), replayGroup);
			// -to-
			//AddMyPlayerReplayToGroupOverride_(this, component.FinishAndGetReplayData(data.finishData_), replayGroup);

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Call && ((MethodInfo)codes[i].operand).Name == "AddReplayToGroup")
				{
					Mod.Instance.Logger.Info($"call AddReplayToGroup @ {i}");

					// Replace: call AddReplayToGroup
					// With:    call AddMyPlayerReplayToGroupOverride_
					codes[i].opcode  = OpCodes.Call;
					codes[i].operand = typeof(ReplayManager__OnAllPlayerEventFinish).GetMethod(nameof(AddMyPlayerReplayToGroupOverride_));

					break;
				}
			}

			return codes.AsEnumerable();
		}

		#region Helpers

		public static void AddMyPlayerReplayToGroupOverride_(ReplayManager replayManager, CarReplayData carReplayData, GameObject group)
		{
			if (carReplayData)
			{
				var carCompoundData = carReplayData.gameObject.GetOrAddComponent<CarReplayDataCompoundData>();
				if (carCompoundData)
				{
					carCompoundData.IsMyPlayer = true;
				}
			}

			replayManager.AddReplayToGroup(carReplayData, group);
		}

		#endregion
	}
}
