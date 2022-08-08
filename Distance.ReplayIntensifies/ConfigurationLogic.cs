using Distance.ReplayIntensifies.Data;
using Distance.ReplayIntensifies.Randomizer;
using Reactor.API.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Distance.ReplayIntensifies
{
	public class ConfigurationLogic : MonoBehaviour
	{
		#region Properties

		private const string EnableSeparateMaxForSelectedReplays_ID = "replays.separate_max_for_selected_replays";
		public bool EnableSeparateMaxForSelectedReplays
		{
			get => Get<bool>(EnableSeparateMaxForSelectedReplays_ID);
			set => Set(EnableSeparateMaxForSelectedReplays_ID, value);
		}

		private const string MaxSelectedReplays_ID = "replays.max_selected_replays";
		public int MaxSelectedReplays
		{
			get => Get<int>(MaxSelectedReplays_ID);
			set => Set(MaxSelectedReplays_ID, value);
		}

		private const string FillWithLocalReplays_ID = "replays.fill_with_local_replays";
		public bool FillWithLocalReplays
		{
			get => Get<bool>(FillWithLocalReplays_ID);
			set => Set(FillWithLocalReplays_ID, value);
		}


		private const string LocalReplayTrimming_ID = "leaderboards.local_replay_trimming";
		public LocalLeaderboardTrimming LocalReplayTrimming
		{
			get => Get<LocalLeaderboardTrimming>(LocalReplayTrimming_ID);
			set => Set(LocalReplayTrimming_ID, value);
		}

		private const string MaxSavedLocalReplays_ID = "leaderboards.max_saved_local_replays";
		public int MaxSavedLocalReplays
		{
			get => Get<int>(MaxSavedLocalReplays_ID);
			set => Set(MaxSavedLocalReplays_ID, value);
		}

		private const string MaxOnlineLeaderboards_ID = "leaderboards.max_online_leaderboards";
		public int MaxOnlineLeaderboards
		{
			get => Get<int>(MaxOnlineLeaderboards_ID);
			set => Set(MaxOnlineLeaderboards_ID, value);
		}


		private const string GhostOutline_ID = "visual.ghost_outline";
		public bool GhostOutline
		{
			get => Get<bool>(GhostOutline_ID);
			set => Set(GhostOutline_ID, value);
		}

		private const string GhostDetailType_ID = "visual.ghost_detail_type";
		public CarLevelOfDetail.Type GhostDetailType
		{
			get => Get<CarLevelOfDetail.Type>(GhostDetailType_ID);
			set => Set(GhostDetailType_ID, value);
		}

		private const string ReplayOutline_ID = "visual.replay_outline";
		public bool ReplayOutline
		{
			get => Get<bool>(ReplayOutline_ID);
			set => Set(ReplayOutline_ID, value);
		}

		private const string ReplayDetailType_ID = "visual.replay_detail_type";
		public CarLevelOfDetail.Type ReplayDetailType
		{
			get => Get<CarLevelOfDetail.Type>(ReplayDetailType_ID);
			set => Set(ReplayDetailType_ID, value);
		}

		private const string MaxDetailLevel_ID = "visual.max_level_of_detail";
		public CarLevelOfDetail.Level MaxLevelOfDetail
		{
			get
			{
				var value = Get<CarLevelOfDetail.Level>(MaxDetailLevel_ID);
				if (value < Mod.MaxMaxLevelOfDetail) value = Mod.MaxMaxLevelOfDetail;
				if (value > Mod.MinMaxLevelOfDetail) value = Mod.MinMaxLevelOfDetail;
				return value;
			}
			set
			{
				if (value < Mod.MaxMaxLevelOfDetail) value = Mod.MaxMaxLevelOfDetail;
				if (value > Mod.MinMaxLevelOfDetail) value = Mod.MinMaxLevelOfDetail;
				Set(MaxDetailLevel_ID, value);
				this.MaxLevelOfDetailCached = value;
			}
		}

		private const string MinDetailLevel_ID = "visual.min_level_of_detail";
		public CarLevelOfDetail.Level MinLevelOfDetail
		{
			get
			{
				var value = Get<CarLevelOfDetail.Level>(MinDetailLevel_ID);
				if (value < Mod.MaxMinLevelOfDetail) value = Mod.MaxMinLevelOfDetail;
				return value;
			}
			set
			{
				if (value < Mod.MaxMinLevelOfDetail) value = Mod.MaxMinLevelOfDetail;
				Set(MinDetailLevel_ID, value);
				this.MinLevelOfDetailCached = value;
			}
		}

		private const string EnableUnrestrictedOpponentColors_ID = "visual.unrestricted_colors";
		public bool EnableUnrestrictedOpponentColors
		{
			get => Get<bool>(EnableUnrestrictedOpponentColors_ID);
			set => Set(EnableUnrestrictedOpponentColors_ID, value);
		}

		private const string UseDataEffectForMode_ID = "visual.use_data_effect_for_mode";
		public GhostOrReplay UseDataEffectForMode
		{
			get => Get<GhostOrReplay>(UseDataEffectForMode_ID);
			set => Set(UseDataEffectForMode_ID, value);
		}


		private const string UseRivalStyleForMode_ID = "visual.use_rival_style_for_mode";
		public GhostOrReplay UseRivalStyleForMode
		{
			get => Get<GhostOrReplay>(UseRivalStyleForMode_ID);
			set => Set(UseRivalStyleForMode_ID, value);
		}

		private const string UseRivalStyleForSelf_ID = "visual.use_rival_style_for_self";
		public bool UseRivalStyleForSelf
		{
			get => Get<bool>(UseRivalStyleForSelf_ID);
			set => Set(UseRivalStyleForSelf_ID, value);
		}

		private const string RivalBrightness_ID = "visual.rival_brightness";
		public float RivalBrightness
		{
			get => Get<float>(RivalBrightness_ID);
			set => Set(RivalBrightness_ID, value);
		}

		private const string RivalOutline_ID = "visual.rival_outline";
		public bool RivalOutline
		{
			get => Get<bool>(RivalOutline_ID, true);
			set => Set(RivalOutline_ID, value);
		}

		private const string RivalDetailType_ID = "visual.rival_detail_type";
		public CarLevelOfDetail.Type RivalDetailType
		{
			get => Get<CarLevelOfDetail.Type>(RivalDetailType_ID);
			set => Set(RivalDetailType_ID, value);
		}

		private const string EnableSteamRivals_ID = "rivals.enabled";
		public bool EnableSteamRivals
		{
			// We can't be having this feature enabled for non-steam builds
			get => SteamworksManager.IsSteamBuild_ && Get<bool>(EnableSteamRivals_ID);
			set => Set(EnableSteamRivals_ID, value);
		}

		private const string HighlightRivalsInLeaderboards_ID = "rivals.highlight_in_leaderboards";
		public bool HighlightRivalsInLeaderboards
		{
			get => Get<bool>(HighlightRivalsInLeaderboards_ID);
			set => Set(HighlightRivalsInLeaderboards_ID, value);
		}

		private const string SteamRivals_ID = "rivals.steam_ids";
		public Dictionary<ulong, string> SteamRivals
		{
			get => Convert<Dictionary<ulong, string>>(SteamRivals_ID, new Dictionary<ulong, string>(), overwriteNull: true);
			private set => Set(SteamRivals_ID, value);
		}


		private const string EnableRandomizedCars_ID = "random.enabled";
		public bool EnableRandomizedCars
		{
			get => Get<bool>(EnableRandomizedCars_ID);
			set => Set(EnableRandomizedCars_ID, value);
		}

		private const string ExtraRandomnessSeed_ID = "random.extra_seed";
		public uint ExtraRandomnessSeed
		{
			get => Get<uint>(ExtraRandomnessSeed_ID);
			set => Set(ExtraRandomnessSeed_ID, value);
		}

		private const string UseRandomCarsFor_ID = "random.local_or_online_cars";
		public LocalOrOnline UseRandomCarsFor
		{
			get => Get<LocalOrOnline>(UseRandomCarsFor_ID);
			set => Set(UseRandomCarsFor_ID, value);
		}

		private const string UseRandomRivalCars_ID = "random.rival_cars";
		public bool UseRandomRivalCars
		{
			get => Get<bool>(UseRandomRivalCars_ID);
			set => Set(UseRandomRivalCars_ID, value);
		}

		private const string RandomRespectBackerCars_ID = "random.respect_backer_cars";
		public bool RandomRespectBackerCars
		{
			get => Get<bool>(RandomRespectBackerCars_ID);
			set => Set(RandomRespectBackerCars_ID, value);
		}

		private const string RandomCarSeedMethod_ID = "random.car_seed_method";
		public RandomSeedMethod RandomCarSeedMethod
		{
			get => Get<RandomSeedMethod>(RandomCarSeedMethod_ID);
			set => Set(RandomCarSeedMethod_ID, value);
		}

		private const string RandomColorSeedMethod_ID = "random.color_seed_method";
		public RandomSeedMethod RandomColorSeedMethod
		{
			get => Get<RandomSeedMethod>(RandomColorSeedMethod_ID);
			set => Set(RandomColorSeedMethod_ID, value);
		}

		private const string RandomCarChoiceMethod_ID = "random.car_choice_method";
		public RandomCarMethod RandomCarChoiceMethod
		{
			get => Get<RandomCarMethod>(RandomCarChoiceMethod_ID);
			set => Set(RandomCarChoiceMethod_ID, value);
		}

		private const string RandomColorChoiceMethod_ID = "random.color_choice_method";
		public RandomColorMethod RandomColorChoiceMethod
		{
			get => Get<RandomColorMethod>(RandomColorChoiceMethod_ID);
			set => Set(RandomColorChoiceMethod_ID, value);
		}

		private const string RandomRequireCarUnlocks_ID = "random.require_car_unlocks";
		public bool RandomRequireCarUnlocks
		{
			get => Get<bool>(RandomRequireCarUnlocks_ID);
			set => Set(RandomRequireCarUnlocks_ID, value);
		}


		private const string RandomCarWeights_ID = "random.car_chances";
		public Dictionary<string, float> RandomCarWeights
		{
			get => Convert<Dictionary<string, float>>(RandomCarWeights_ID, new Dictionary<string, float>(), overwriteNull: true);
			private set => Set(RandomCarWeights_ID, value);
		}

		private const string RandomCustomCarsDefaultWeight_ID = "random.custom_cars_default_chance";
		public float RandomCustomCarsDefaultWeight
		{
			get => Get<float>(RandomCustomCarsDefaultWeight_ID);
			set => Set(RandomCustomCarsDefaultWeight_ID, value);
		}

		private const string RandomCustomCarsSplitDefaultWeight_ID = "random.custom_cars_split_default_chance";
		public bool RandomCustomCarsSplitDefaultWeight
		{
			get => Get<bool>(RandomCustomCarsSplitDefaultWeight_ID);
			set => Set(RandomCustomCarsSplitDefaultWeight_ID, value);
		}


		private const string ReplayModeDisableCinematicCameras_ID = "replaymode.disable_cinematic_cameras";
		public bool ReplayModeDisableCinematicCameras
		{
			get => Get<bool>(ReplayModeDisableCinematicCameras_ID);
			set => Set(ReplayModeDisableCinematicCameras_ID, value);
		}

		private const string ReplayModePauseAtStart_ID = "replaymode.pause_at_start";
		public bool ReplayModePauseAtStart
		{
			get => Get<bool>(ReplayModePauseAtStart_ID);
			set => Set(ReplayModePauseAtStart_ID, value);
		}

		private const string FinishPreSpectateTime_ID = "replaymode.pre_spectate_time";
		public float FinishPreSpectateTime
		{
			get => Get<float>(FinishPreSpectateTime_ID);
			set => Set(FinishPreSpectateTime_ID, value);
		}

		#endregion

		#region Cached

		// Cached property values for faster accessing.
		public CarLevelOfDetail.Level MaxLevelOfDetailCached { get; private set; }
		public CarLevelOfDetail.Level MinLevelOfDetailCached { get; private set; }

		#endregion

		#region Helpers

		public CarLevelOfDetail.Type GetCarDetailType(bool isGhost, bool isCarRival)
		{
			if (isCarRival)
			{
				return this.RivalDetailType;
			}
			return (isGhost) ? this.GhostDetailType : this.ReplayDetailType;
		}

		public bool GetCarOutline(bool isGhost, bool isCarRival)
		{
			if (isCarRival)
			{
				return this.RivalOutline;
			}
			return (isGhost) ? this.GhostOutline : this.ReplayOutline;
		}

		// Determines if this car should be displayed as a Steam Rival (which accounts for settings like 'Use rival style for ghosts/replays', etc.).
		public bool IsCarSteamRival(bool isGhost, long userID) => IsCarSteamRival(isGhost, unchecked((ulong)userID));

		public bool IsCarSteamRival(bool isGhost, ulong userID)
		{
			if (this.EnableSteamRivals && this.UseRivalStyleForMode.HasGhostOrReplay(isGhost))
			{
				return IsSteamRival(userID, false);
			}
			return false;
		}

		public float GetCarTypeChance(string carName)
		{
			if (this.RandomCarWeights.TryGetValue(carName, out float weight))
			{
				return weight;
			}
			return 0f;
		}

		public void SetCarTypeChance(string carName, float weight, bool autoSave = true)
		{
			var randomCarChances = this.RandomCarWeights;
			if (!randomCarChances.TryGetValue(carName, out float oldWeight) || oldWeight != weight)
			{
				randomCarChances[carName] = weight;
				if (autoSave)
				{
					Save();
				}
			}
		}

		public bool IsCarRandomnessEnabled(bool isOnline, bool isCarRival, string carName)
		{
			if (this.EnableRandomizedCars)
			{
				if (isOnline && this.RandomRespectBackerCars && RandomCarType.IsBackerCar(carName))
				{
					// Only count setting when used for online cars (we still want backers to be able to randomize their own local replays).
					return false;
				}
				else if (isCarRival)
				{
					return this.UseRandomRivalCars;
				}
				else if (isOnline)
				{
					return this.UseRandomCarsFor.HasFlag(LocalOrOnline.Online_Replays);
				}
				else
				{
					return this.UseRandomCarsFor.HasFlag(LocalOrOnline.Local_Replays);
				}
			}
			return false;
		}

		#endregion

		#region Steam Rivals

		// The `excludeSelf` parameter exists in-case there are situations where we want to identify rivals for non-ghost/replay reasons.
		public bool IsSteamRival(long userID, bool excludeSelf = false) => IsSteamRival(unchecked((ulong)userID), excludeSelf);

		public bool IsSteamRival(ulong userID, bool excludeSelf = false)
		{
			if (SteamworksManager.GetSteamID() == userID)
			{
				return !excludeSelf && this.UseRivalStyleForSelf; // SteamRivals ignores your own user ID in the list.
			}
			return this.SteamRivals.ContainsKey(userID);
		}

		// For getting the name comment attached to a rival. These currently aren't used by the mod though.
		public bool TryGetSteamRival(long userID, out string nameComment) => TryGetSteamRival(unchecked((ulong)userID), out nameComment);

		public bool TryGetSteamRival(ulong userID, out string nameComment)
		{
			return this.SteamRivals.TryGetValue(userID, out nameComment);
		}

		public bool AddSteamRival(long userID, string nameComment, bool autoSave = true) => AddSteamRival(unchecked((ulong)userID), nameComment, autoSave);

		public bool AddSteamRival(ulong userID, string nameComment, bool autoSave = true)
		{
			if (nameComment == null)
			{
				nameComment = string.Empty; // Default to empty string I guess? It doesn't really matter either way, but would be more user friendly.
			}

			var steamRivals = this.SteamRivals;
			if (!steamRivals.ContainsKey(userID))
			{
				steamRivals[userID] = nameComment; // Name comment to make identifying users in Config.json easier.
				if (autoSave)
				{
					Save();
				}
				return true;
			}
			return false;
		}

		public bool RemoveSteamRival(long userID, bool autoSave = true) => RemoveSteamRival(unchecked((ulong)userID), autoSave);

		public bool RemoveSteamRival(ulong userID, bool autoSave = true)
		{
			if (this.SteamRivals.Remove(userID))
			{
				if (autoSave)
				{
					Save();
				}
				return true;
			}
			return false;
		}

		// Shorthand function to reduce the time spent on lookup when a large number of userIDs are in a single collection.
		public int CountSteamRivals(IEnumerable<long> userIDs, bool countSelf = true)
		{
			return CountSteamRivals(userIDs.Select((userID) => unchecked((ulong)userID)), countSelf);
		}

		public int CountSteamRivals(IEnumerable<ulong> userIDs, bool countSelf = true)
		{
			//var useRivalStyleForSelf = this.UseRivalStyleForSelf;
			ulong selfID = SteamworksManager.GetSteamID();
			var steamRivals = this.SteamRivals;

			return userIDs.Count((userID) => (userID == selfID ? countSelf : steamRivals.ContainsKey(userID)));
		}

		#endregion


		#region Randomized Cars

		private int unfixedRandomCount = 0;

		private System.Random GetRandom(int replaySeed, int placementSeed, RandomSeedMethod seedMethod, int skipCount)
		{
			int seed;
			switch (seedMethod)
			{
			case RandomSeedMethod.By_Replay:
				seed = replaySeed;
				break;
			case RandomSeedMethod.By_Placement:
				seed = placementSeed;
				break;
			case RandomSeedMethod.Always_Random:
			default:
				// Default constructor seed for System.Random, combined with an increment
				//  to ensure we're not creating multiple RNGs on the same tick.
				seed = unchecked(Environment.TickCount + this.unfixedRandomCount++);
				break;
			}
			System.Random rng = new System.Random(seed ^ unchecked((int)this.ExtraRandomnessSeed));

			for (int i = 0; i < skipCount; i++)
			{
				rng.NextDouble();
			}
			return rng;
		}

		public CarReplayData.CarData ChooseRandomCarData(CarReplayData.CarData origCarData, int replaySeed, int placementSeed,
														 List<RandomCarType> carTypes, List<RandomColorPreset> colorPresets)
		{
			var carRng = GetRandom(replaySeed, placementSeed, this.RandomCarSeedMethod, 0);
			var colorRng = GetRandom(replaySeed, placementSeed, this.RandomColorSeedMethod, 1);

			// ==== Choose our car type ====

			if (!RandomCarType.TryCreate(origCarData.name_, 1f, 1, false, out var carType))
			{
				carType = RandomCarType.DefaultCarType;
			}

			var carMethod = this.RandomCarChoiceMethod;
			if (carMethod.IsCarTypes())
			{
				if (carTypes != null)
				{
					int carTypeIndex = carTypes.Count - 1; // Default to end in very unlikely scenario of rolling exactly 1.0.

					// Use doubles starting here for higher precision.
					double totalWeight = carTypes.Sum((x) => (double)x.Weight);

					double end = 0.0;
					double choice = 0.0;
					if (carMethod != RandomCarMethod.Car_Types_Ordered)
					{
						choice = carRng.NextDouble() * totalWeight;
					}

					for (int i = 0; i < carTypes.Count; i++)
					{
						end += carTypes[i].Weight;
						if (choice < end)
						{
							carTypeIndex = i;
							break;
						}
					}

					carType = carTypes[carTypeIndex];
					// Remove from the pool of cars to choose from if needed.
					if (carMethod.IsAvoidDuplicates() && carType.MaxCount > 0)
					{
						if (--carType.RemainingCount <= 0)
						{
							carTypes.RemoveAt(carTypeIndex);
						}
					}
				}
			}

			// ==== Choose our car colors ====

			CarColors carColors = origCarData.colors_;
			var colorMethod = this.RandomColorChoiceMethod;
			if (colorMethod == RandomColorMethod.Default_Colors)
			{
				carColors = carType.DefaultColors;
			}
			else if (colorMethod.IsColorPresets())
			{
				if (colorPresets != null)
				{
					int colorPresetIndex = colorPresets.Count - 1; // Default to end in very unlikely scenario of rolling exactly 1.0.

					double totalWeight = colorPresets.Sum((x) => (double)x.Weight);

					double end = 0.0;
					double choice = 0.0;
					if (colorMethod != RandomColorMethod.Color_Presets_Ordered)
					{
						choice = colorRng.NextDouble() * totalWeight;
					}

					for (int i = 0; i < colorPresets.Count; i++)
					{
						end += colorPresets[i].Weight;
						if (choice < end)
						{
							colorPresetIndex = i;
							break;
						}
					}

					var colorPreset = colorPresets[colorPresetIndex];
					if (colorPreset.IsDefault)
					{
						carColors = carType.DefaultColors;
					}
					else
					{
						carColors = colorPreset.Colors;
					}

					// Remove from the pool of colors to choose from if needed.
					if (colorMethod.IsAvoidDuplicates() && colorPreset.MaxCount > 0)
					{
						if (--colorPreset.RemainingCount <= 0)
						{
							colorPresets.RemoveAt(colorPresetIndex);
						}
					}
				}
			}
			else if (colorMethod == RandomColorMethod.HSV)
			{
				for (ColorChanger.ColorType i = 0; i < ColorChanger.ColorType.Size_; i++)
				{
					// HSV seems to give a better random spread of colors than RGB.
					// see: <https://stackoverflow.com/a/3135179/7517185>
					carColors[i] = Color.HSVToRGB((float)colorRng.NextDouble(),
												  (float)colorRng.NextDouble(),
												  (float)colorRng.NextDouble());
				}
			}
			else if (colorMethod == RandomColorMethod.RGB)
			{
				for (ColorChanger.ColorType i = 0; i < ColorChanger.ColorType.Size_; i++)
				{
					carColors[i] = new Color((float)colorRng.NextDouble(),
											 (float)colorRng.NextDouble(),
											 (float)colorRng.NextDouble());
				}
			}

			return new CarReplayData.CarData(carType.Name, carColors);
		}

		public List<RandomColorPreset> LoadRandomColorPresets()
		{
			var colorPresets = RandomColorPreset.LoadAllColorPresets(defaultMaxCount: 1);

			if (this.RandomColorSeedMethod == RandomSeedMethod.By_Placement)
			{
				// Skip default color preset when coloring by placement (since the color varies by car).
				colorPresets.RemoveAll((x) => x.IsDefault);
			}
			
			return colorPresets;
		}

		public List<RandomCarType> LoadRandomCarTypes()
		{
			Dictionary<string, RandomCarType> randomCarTypes = new Dictionary<string, RandomCarType>();
			int explicitCustomCount = 0;
			bool requireUnlocks = this.RandomRequireCarUnlocks;

			foreach (var carWeightPair in this.RandomCarWeights)
			{
				if (RandomCarType.TryCreate(carWeightPair.Key, carWeightPair.Value, 1, requireUnlocks, out RandomCarType carType))
				{
					randomCarTypes.Add(carType.Name, carType);
					if (!carType.IsVanilla)
					{
						explicitCustomCount++;
					}
				}
			}

			// Exclude number of explicitly included custom cars in settings file.
			//int implicitCustomCount = knownCars.Count - Mod.VanillaCarChances.Count - explicitCustomCount;
			int implicitCustomCount = RandomCarType.CustomCarsCount - explicitCustomCount;

			float implicitCustomWeight = this.RandomCustomCarsDefaultWeight;
			if (implicitCustomCount > 0 && implicitCustomWeight > 0f)
			{
				if (this.RandomCustomCarsSplitDefaultWeight)
				{
					// The chance of choosing a custom car is split up equally across all custom cars.
					implicitCustomWeight /= implicitCustomCount;
				}

				foreach (string customCarName in RandomCarType.CustomCarNames)
				{
					if (!randomCarTypes.ContainsKey(customCarName) &&
						RandomCarType.TryCreate(customCarName, implicitCustomWeight, 1, requireUnlocks, out RandomCarType carType))
					{
						randomCarTypes.Add(carType.Name, carType);
					}
				}
			}

			var randomCarTypeList = randomCarTypes.Values.ToList();
			randomCarTypeList.Sort(); // Sort using RandomCarInfo IComparable interface.

			if (randomCarTypeList.Count == 0)
			{
				randomCarTypeList.Add(RandomCarType.DefaultCarType);
			}

			return randomCarTypeList;
		}

		// Default weights for known vanilla cars.
		public Dictionary<string, float> GetDefaultCarWeights()
		{
			return new Dictionary<string, float>
			{
				{ "Spectrum",    1.00f * Mod.MaxRandomWeight },
				{ "Archive",     0.75f * Mod.MaxRandomWeight }, // (Adventure complete car)
				{ "Interceptor", 0.50f * Mod.MaxRandomWeight }, // (Nitronic Rush car)
				{ "Encryptor",   0.08f * Mod.MaxRandomWeight }, // (Doot car)
				{ "Halcyon",     0.05f * Mod.MaxRandomWeight }, // (Shiny Spectrum car)
				//{ "Catalyst",    0.00f }, // (Kickstarter Backer car, disabled for those that don't have this car unlocked)
			};
		}

		#endregion

		internal Settings Config;

		public event Action<ConfigurationLogic> OnChanged;

		private void Load()
		{
			Config = new Settings("Config");
		}

		public void Awake()
		{
			Load();

			// Assign default settings (if not already assigned).
			Get(EnableSeparateMaxForSelectedReplays_ID, false);
			Get(MaxSelectedReplays_ID, 20);
			Get(FillWithLocalReplays_ID, false);

			Get(LocalReplayTrimming_ID, LocalLeaderboardTrimming.Current);
			Get(MaxSavedLocalReplays_ID, 500);
			Get(MaxOnlineLeaderboards_ID, 1000);

			Get(GhostOutline_ID, true);
			Get(GhostDetailType_ID, CarLevelOfDetail.Type.Ghost);
			Get(ReplayOutline_ID, false);
			Get(ReplayDetailType_ID, CarLevelOfDetail.Type.Replay);
			// Assign back to non-cached properties to handle automatic clamping and caching.
			this.MaxLevelOfDetail = Get(MaxDetailLevel_ID, Mod.MaxMaxLevelOfDetail);
			this.MinLevelOfDetail = Get(MinDetailLevel_ID, CarLevelOfDetail.Level.Speck);
			Get(EnableUnrestrictedOpponentColors_ID, false);
			Get(UseDataEffectForMode_ID, GhostOrReplay.Replay_Mode);

			// Experimental (disabled by default)
			Get(EnableSteamRivals_ID, false);
			Convert(SteamRivals_ID, new Dictionary<ulong, string>(), overwriteNull: true);
			Get(HighlightRivalsInLeaderboards_ID, true);
			Get(UseRivalStyleForMode_ID, GhostOrReplay.Ghost_Mode);
			Get(UseRivalStyleForSelf_ID, false);
			Get(RivalBrightness_ID, 1.0f);
			Get(RivalOutline_ID, true);
			Get(RivalDetailType_ID, CarLevelOfDetail.Type.Networked);

			// Randomized Cars
			Get(EnableRandomizedCars_ID, false);
			Get(UseRandomCarsFor_ID, LocalOrOnline.Local_Replays);
			Get(UseRandomRivalCars_ID, false);
			Get(RandomRespectBackerCars_ID, true);

			Get(ExtraRandomnessSeed_ID, (uint)0u);
			Get(RandomCarSeedMethod_ID, RandomSeedMethod.By_Replay);
			Get(RandomColorSeedMethod_ID, RandomSeedMethod.By_Replay);
			Get(RandomCarChoiceMethod_ID, RandomCarMethod.Car_Types);
			Get(RandomColorChoiceMethod_ID, RandomColorMethod.Color_Presets);
			Get(RandomRequireCarUnlocks_ID, true);

			Convert(RandomCarWeights_ID, GetDefaultCarWeights(), overwriteNull: true);
			Get(RandomCustomCarsDefaultWeight_ID, 0.0f);
			Get(RandomCustomCarsSplitDefaultWeight_ID, false);

			// Replay Mode
			Get(ReplayModeDisableCinematicCameras_ID, false);
			Get(ReplayModePauseAtStart_ID, false);
			Get(FinishPreSpectateTime_ID, 5f);

			// Save settings, and any defaults that may have been added.
			Save();
		}

		public T Get<T>(string key, T @default = default)
		{
			return Config.GetOrCreate(key, @default);
		}

		public void Set<T>(string key, T value)
		{
			Config[key] = value;
			Save();
		}

		public T Convert<T>(string key, T @default = default, bool overwriteNull = false)
		{
			// Assign the object back after conversion, this allows for deep nested settings
			//  that can be preserved and updated without reassigning to the root property.
			var value = Config.GetOrCreate(key, @default);
			if (overwriteNull && value == null)
			{
				value = @default;
			}
			Config[key] = value;
			return value;
		}

		public void Save()
		{
			Config?.Save();
			OnChanged?.Invoke(this);
		}
	}
}
