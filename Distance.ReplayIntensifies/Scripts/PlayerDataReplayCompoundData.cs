using System;
using UnityEngine;

namespace Distance.ReplayIntensifies.Scripts
{
	public class PlayerDataReplayCompoundData : MonoBehaviour
	{
		public PlayerDataReplay Player { get; internal set; }

		// When not in replay mode.
		public bool IsGhostMode { get; internal set; }

		public CarLevelOfDetail.Type DetailType { get; internal set; }

		public bool HasOutline { get; internal set; }

		public bool IsRival { get; internal set; }

		// Data materialization effect seen when a car spawns.
		public bool ShowDataEffect { get; internal set; }

		// This car acts like a ghost and will not affect the environment in any way.
		// Will differ from `IsGhostMode` after the start of `InitPlayerDataReplay` if `simulateNetworkCar_` is true.
		public bool IsGhostBehavior => this.Player.IsGhost_;

		public bool IsGhostVisual => this.DetailType == CarLevelOfDetail.Type.Ghost;

		// We want to calculate this property on the fly, since the ghost brightness setting can be changed mid-game.
		public float GetOutlineBrightness()
		{
			if (!this.HasOutline) // !this.OriginalIsGhost // !this.IsGhost
			{
				// For now treat 'no outline' as using the default brightness of 1.0,
				//  since this brightness is also applied for jet flames and wing trails.
				// TODO: Should replays just use the same outline brightness as ghosts??
				// TODO: If not, should brightness be decided based on being in Replay Mode vs. racing ghosts?
				return 1f;
			}
			else if (this.IsRival)
			{
				return Mod.Instance.Config.RivalBrightness;
			}
			else
			{
				return this.Player.replaySettings_.GhostBrightness_;
			}
		}

		public static PlayerDataReplayCompoundData Create(PlayerDataReplay playerDataReplay, CarReplayData data, bool isGhost)
		{
			bool isRival = Mod.Instance.Config.IsCarSteamRival(isGhost, data.steamID_);
			CarLevelOfDetail.Type detailType = Mod.Instance.Config.GetCarDetailType(isGhost, isRival);
			bool hasOutline = Mod.Instance.Config.GetCarOutline(isGhost, isRival);
			bool showDataEffect = Mod.Instance.Config.ShowDataEffectInGhostMode;

			// This isn't assigned until the start of `InitPlayerDataReplay`, so use a local variable.
			bool isGhostBehavior = (isGhost && !PlayerDataReplay.simulateNetworkCar_);

			var compoundData = playerDataReplay.gameObject.AddComponent<PlayerDataReplayCompoundData>();
			compoundData.Player = playerDataReplay;
			compoundData.IsGhostMode = isGhost; // Original state
			compoundData.DetailType = detailType;
			compoundData.HasOutline = hasOutline;
			compoundData.IsRival = isRival;
			// Never show data effect for ghost visuals. Only show in ghost mode if the settings are configured to show it.
			compoundData.ShowDataEffect = !compoundData.IsGhostVisual && (!isGhostBehavior || showDataEffect);

			return compoundData;
		}
	}
}
