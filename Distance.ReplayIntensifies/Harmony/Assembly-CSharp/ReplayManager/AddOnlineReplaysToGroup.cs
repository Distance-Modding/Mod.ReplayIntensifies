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
	/// Patch to mark replays that were loaded from online leaderboards.
	/// </summary>
	/// <remarks>
	/// Required For: Use Random Online Cars.
	/// </remarks>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.AddOnlineReplaysToGroup))]
	internal static class ReplayManager__AddOnlineReplaysToGroup
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//this.AddReplayToGroup(carReplayData, group);
			// -to-
			//AddOnlineReplayToGroupOverride_(this, carReplayData, group);

			for (int i = 0; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Call && ((MethodInfo)codes[i].operand).Name == "AddReplayToGroup")
				{
					Mod.Instance.Logger.Info($"call AddReplayToGroup @ {i}");

					// Replace: call AddReplayToGroup
					// With:    call AddOnlineReplayToGroupOverride_
					codes[i].opcode  = OpCodes.Call;
					codes[i].operand = typeof(ReplayManager__AddOnlineReplaysToGroup).GetMethod(nameof(AddOnlineReplayToGroupOverride_));

					break;
				}
			}

			return codes.AsEnumerable();
		}

		#region Helpers

		public static void AddOnlineReplayToGroupOverride_(ReplayManager replayManager, CarReplayData carReplayData, GameObject group)
		{
			if (carReplayData)
			{
				var carCompoundData = carReplayData.gameObject.GetOrAddComponent<CarReplayDataCompoundData>();
				if (carCompoundData)
				{
					carCompoundData.IsOnline = true;
				}
			}

			replayManager.AddReplayToGroup(carReplayData, group);
		}

		#endregion
	}
}
