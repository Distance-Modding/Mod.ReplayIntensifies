using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using System;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to change the value of <see cref="PlayerDataReplay.simulateNetworkCar_"/> for the duration of the function.
	/// </summary>
	/// <remarks>
	/// Required For: Car Visual Style (part 1/4).
	/// </remarks>
	[HarmonyPatch(typeof(PlayerDataReplay), nameof(PlayerDataReplay.CreatePlayerDataReplay))]
	internal static class PlayerDataReplay__CreatePlayerDataReplay
	{
		[HarmonyPrefix]
		internal static bool Prefix(ref PlayerDataReplay __result, GameObject playerDataPrefab, CarReplayData data, bool isGhost)
		{
			GameObject gameObject = playerDataPrefab.Duplicate();
			PlayerDataReplay playerDataReplay = gameObject.GetComponent<PlayerDataReplay>();

			CarLevelOfDetail.Type detailType = Mod.Instance.Config.GetCarDetailType(isGhost);
			bool hasOutline = Mod.Instance.Config.GetCarOutline(isGhost);

			var compoundData = playerDataReplay.gameObject.AddComponent<PlayerDataReplayCompoundData>();
			compoundData.Player = playerDataReplay;
			compoundData.OriginalIsGhost = isGhost; // Original state
			compoundData.DetailType = detailType;
			compoundData.HasOutline = hasOutline;

			isGhost = compoundData.IsGhost; // (detailType == CarLevelOfDetail.Type.Ghost);

			//Mod.Instance.Logger.Info($"IsGhost = {compoundData.IsGhost}, IsDefault = {compoundData.IsDefault}, DetailType = {compoundData.DetailType}, HasOutline = {compoundData.HasOutline}, OriginalIsGhost = {compoundData.OriginalIsGhost}");

			playerDataReplay.InitPlayerDataReplay(data, isGhost);

			__result = playerDataReplay;

			return false; // Don't run original method
		}
	}
}
