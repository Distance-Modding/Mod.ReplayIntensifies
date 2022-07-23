using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to change the value of <see cref="PlayerDataReplay.simulateNetworkCar_"/> for the duration of the function.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 1/5).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.CreatePlayerDataReplay))]
	internal static class PlayerDataReplay__CreatePlayerDataReplay
	{
		// Prefix implementation version:
		/*[HarmonyPrefix]
		internal static bool Prefix(ref PlayerDataReplay __result, GameObject playerDataPrefab, CarReplayData data, bool isGhost)
		{
			GameObject gameObject = playerDataPrefab.Duplicate();
			PlayerDataReplay playerDataReplay = gameObject.GetComponent<PlayerDataReplay>();

			bool isRival = Mod.Instance.Config.IsSteamRivalForMode(isGhost, data.steamID_);
			CarLevelOfDetail.Type detailType = Mod.Instance.Config.GetCarDetailType(isGhost, isRival);
			bool hasOutline = Mod.Instance.Config.GetCarOutline(isGhost, isRival);

			var compoundData = playerDataReplay.gameObject.AddComponent<PlayerDataReplayCompoundData>();
			compoundData.Player = playerDataReplay;
			compoundData.OriginalIsGhost = isGhost; // Original state
			compoundData.DetailType = detailType;
			compoundData.HasOutline = hasOutline;
			compoundData.IsRival = isRival;

			playerDataReplay.outlineBrightness_ = compoundData.GetOutlineBrightness();

			//Mod.Instance.Logger.Info($"IsGhost = {compoundData.IsGhost}, DetailType = {compoundData.DetailType}, HasOutline = {compoundData.HasOutline}, OriginalIsGhost = {compoundData.OriginalIsGhost}");

			isGhost = compoundData.IsGhost; // (detailType == CarLevelOfDetail.Type.Ghost);
			playerDataReplay.InitPlayerDataReplay(data, isGhost);

			__result = playerDataReplay;

			return false; // Don't run original method
		}*/


		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			// Setup compound data before initializing player data replay.
			//PlayerDataReplay playerDataReplay = gameObject.GetComponent<PlayerDataReplay>();
			//isGhost = SetupCompoundData_(playerDataReplay, data, isGhost);
			//playerDataReplay.InitPlayerDataReplay(data, isGhost);

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 3; i < codes.Count; i++)
			{
				if (codes[i].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i].operand).Name == "InitPlayerDataReplay")
				{
					Mod.Instance.Logger.Info($"call InitPlayerDataReplay @ {i}");

					// NOTE: `SetupCompoundData_` has the same arguments as `InitPlayerDataReplay`,
					//       so we can just copy the instructions.
					// Insert:  ldloc. (playerDataReplay)
					// Insert:  ldarg.1
					// Insert:  ldarg.2
					// Insert:  call SetupCompoundData_
					// Insert:  starg.s 2 (isGhost)
					// Before:  ldloc. (playerDataReplay)
					// Before:  ldarg.1
					// Before:  ldarg.2
					// Before:  callvirt InitPlayerDataReplay
					codes.InsertRange(i - 3, new CodeInstruction[]
					{
						new CodeInstruction(codes[i - 3].opcode, codes[i - 3].operand), // ldloc. (playerDataReplay)
						new CodeInstruction(OpCodes.Ldarg_1, null),
						new CodeInstruction(OpCodes.Ldarg_2, null),
						new CodeInstruction(OpCodes.Call, typeof(PlayerDataReplay__CreatePlayerDataReplay).GetMethod(nameof(SetupCompoundData_))),
						new CodeInstruction(OpCodes.Starg_S, (byte)2), // starg. (isGhost)
					});

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		// Trailing underscore added since there's no HarmonyIgnore attribute.
		// Returns the new value of isGhost.
		public static bool SetupCompoundData_(PlayerDataReplay playerDataReplay, CarReplayData data, bool isGhost)
		{
			bool isRival = Mod.Instance.Config.IsCarSteamRival(isGhost, data.steamID_);
			CarLevelOfDetail.Type detailType = Mod.Instance.Config.GetCarDetailType(isGhost, isRival);
			bool hasOutline = Mod.Instance.Config.GetCarOutline(isGhost, isRival);

			var compoundData = playerDataReplay.gameObject.AddComponent<PlayerDataReplayCompoundData>();
			compoundData.Player = playerDataReplay;
			compoundData.OriginalIsGhost = isGhost; // Original state
			compoundData.DetailType = detailType;
			compoundData.HasOutline = hasOutline;
			compoundData.IsRival = isRival;

			playerDataReplay.outlineBrightness_ = compoundData.GetOutlineBrightness();

			//Mod.Instance.Logger.Info($"IsGhost = {compoundData.IsGhost}, DetailType = {compoundData.DetailType}, HasOutline = {compoundData.HasOutline}, OriginalIsGhost = {compoundData.OriginalIsGhost}");

			return compoundData.IsGhost; // (detailType == CarLevelOfDetail.Type.Ghost);
		}

		#endregion
	}
}
