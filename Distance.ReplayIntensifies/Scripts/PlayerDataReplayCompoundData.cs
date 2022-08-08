using Distance.ReplayIntensifies.Data;
using Distance.ReplayIntensifies.Helpers;
using Distance.ReplayIntensifies.Randomizer;
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

		public bool IsOnline { get; internal set; }

		public bool IsMyPlayer { get; internal set; }

		public bool DidNotFinish { get; internal set; }

		public int Placement { get; internal set; }

		public CarReplayData.CarData OriginalCarData { get; internal set; }

		public CarReplayData.CarData CarData { get; internal set; }

		public bool IsRandomnessEnabled { get; internal set; }

		public int ReplaySeed { get; internal set; }

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

			// This isn't assigned until the start of `InitPlayerDataReplay`, so use a local variable.
			bool isGhostBehavior = (isGhost && !PlayerDataReplay.simulateNetworkCar_);

			bool showDataEffect = Mod.Instance.Config.UseDataEffectForMode.HasGhostOrReplay(isGhostBehavior);

			var compoundData = playerDataReplay.gameObject.AddComponent<PlayerDataReplayCompoundData>();
			compoundData.Player = playerDataReplay;
			compoundData.IsGhostMode = isGhost; // Original state
			compoundData.DetailType = detailType;
			compoundData.HasOutline = hasOutline;
			compoundData.IsRival = isRival;
			compoundData.ShowDataEffect = !compoundData.IsGhostVisual && showDataEffect;
			compoundData.CarData = compoundData.OriginalCarData = data.carData_;

			var carCompoundData = data.GetComponent<CarReplayDataCompoundData>();
			if (carCompoundData)
			{
				compoundData.IsOnline = carCompoundData.IsOnline;
				compoundData.IsMyPlayer = carCompoundData.IsMyPlayer;
				compoundData.DidNotFinish = carCompoundData.DidNotFinish;
				compoundData.Placement = carCompoundData.Placement;
			}

			compoundData.IsRandomnessEnabled = Mod.Instance.Config.IsCarRandomnessEnabled(compoundData.IsOnline, isRival, data.carData_.name_);
			// Never randomize your own replay of your most recent run (so that the car remains the same when clicking view replay).
			if (compoundData.IsMyPlayer)
			{
				compoundData.IsRandomnessEnabled = false;
			}

			if (compoundData.IsRandomnessEnabled)
			{
				// Determine a fixed seed to use for the replay car's randomness.
				if (Mod.Instance.Config.RandomCarSeedMethod   == RandomSeedMethod.By_Replay ||
					Mod.Instance.Config.RandomColorSeedMethod == RandomSeedMethod.By_Replay)
				{
					//System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();

					// This seems excessive, maybe only take a max number of bytes?
					// CRC-32 is a much more reliable hash than what's used by Mono.String.GetHashCode(),
					// The calculation duration of the hash is double that of Mono.String.GetHashCode(),
					//  but the worst you'll get is like 0.4 seconds for 10+ hours of replay data(?)
					int seed = (int)Crc.Initial32;
					seed = Crc.Hash32(data.StateBuffer_, seed);
					seed = Crc.Hash32(data.EventBuffer_, seed);
					compoundData.ReplaySeed = seed;

					//watch.Stop();
					//int length = data.StateBuffer_.Length + data.EventBuffer_.Length;
					//Mod.Instance.Logger.Debug($"Took {watch.ElapsedMilliseconds}ms to hash {length} bytes of replay data");
				}

				// Choose the random car type and colors to use.
				var rmCompoundData = G.Sys.ReplayManager_.GetComponent<ReplayManagerCompoundData>();
				if (rmCompoundData)
				{
					compoundData.CarData = rmCompoundData.ChooseRandomCarData(data.carData_, compoundData.ReplaySeed, compoundData.Placement);
				}
			}

			return compoundData;
		}
	}
}
