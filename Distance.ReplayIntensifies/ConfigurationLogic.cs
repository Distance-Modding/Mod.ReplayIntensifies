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

		private const string ShowDataEffectInGhostMode_ID = "visual.data_effect_in_ghost_mode";
		public bool ShowDataEffectInGhostMode
		{
			get => Get<bool>(ShowDataEffectInGhostMode_ID);
			set => Set(ShowDataEffectInGhostMode_ID, value);
		}


		private const string UseRivalStyleForGhosts_ID = "visual.use_rival_style_for_ghosts";
		public bool UseRivalStyleForGhosts
		{
			get => Get<bool>(UseRivalStyleForGhosts_ID);
			set => Set(UseRivalStyleForGhosts_ID, value);
		}

		private const string UseRivalStyleForReplays_ID = "visual.use_rival_style_for_replays";
		public bool UseRivalStyleForReplays
		{
			get => Get<bool>(UseRivalStyleForReplays_ID);
			set => Set(UseRivalStyleForReplays_ID, value);
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

		private const string FixedRandomness_ID = "random.fixed_randomness";
		public bool FixedRandomness
		{
			get => Get<bool>(FixedRandomness_ID);
			set => Set(FixedRandomness_ID, value);
		}


		private const string RandomOfflineCars_ID = "random.offline_cars";
		public bool RandomOfflineCars
		{
			get => Get<bool>(RandomOfflineCars_ID);
			set => Set(RandomOfflineCars_ID, value);
		}

		private const string RandomOnlineCars_ID = "random.online_cars";
		public bool RandomOnlineCars
		{
			get => Get<bool>(RandomOnlineCars_ID);
			set => Set(RandomOnlineCars_ID, value);
		}

		private const string RandomRivalCars_ID = "random.rival_cars";
		public bool RandomRivalCars
		{
			get => Get<bool>(RandomRivalCars_ID);
			set => Set(RandomRivalCars_ID, value);
		}

		private const string RespectBackerCars_ID = "random.respect_backer_cars";
		public bool RespectBackerCars
		{
			get => Get<bool>(RespectBackerCars_ID);
			set => Set(RespectBackerCars_ID, value);
		}


		private const string RandomCarChances_ID = "random.car_chances";
		public Dictionary<string, float> RandomCarChances
		{
			get => Convert<Dictionary<string, float>>(RandomCarChances_ID, RandomCarType.VanillaCarChances, overwriteNull: true);
			private set => Set(RandomCarChances_ID, value);
		}

		private const string RandomCustomCarsChance_ID = "random.custom_cars_chance";
		public float RandomCustomCarsChance
		{
			get => Get<float>(RandomCustomCarsChance_ID);
			set => Set(RandomCustomCarsChance_ID, value);
		}

		private const string IndividualRandomCustomCarsChance_ID = "random.custom_cars_chance_individual";
		public bool IndividualRandomCustomCarsChance
		{
			get => Get<bool>(IndividualRandomCustomCarsChance_ID);
			set => Set(IndividualRandomCustomCarsChance_ID, value);
		}


		private const string RandomCarMethod_ID = "random.car_method";
		public RandomCarMethod RandomCarMethod
		{
			get => Get<RandomCarMethod>(RandomCarMethod_ID);
			set => Set(RandomCarMethod_ID, value);
		}

		private const string RandomColorMethod_ID = "random.color_method";
		public RandomColorMethod RandomColorMethod
		{
			get => Get<RandomColorMethod>(RandomColorMethod_ID);
			set => Set(RandomColorMethod_ID, value);
		}

		private const string RandomCarByPlacement_ID = "random.car_by_placement";
		public bool RandomCarByPlacement
		{
			get => Get<bool>(RandomCarByPlacement_ID);
			set => Set(RandomCarByPlacement_ID, value);
		}

		private const string RandomColorByPlacement_ID = "random.color_by_placement";
		public bool RandomColorByPlacement
		{
			get => Get<bool>(RandomColorByPlacement_ID);
			set => Set(RandomColorByPlacement_ID, value);
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
			if (this.EnableSteamRivals &&
				((isGhost && this.UseRivalStyleForGhosts) || (!isGhost && this.UseRivalStyleForReplays)))
			{
				return IsSteamRival(userID, false);
			}
			return false;
		}

		public float GetCarTypeChance(string carName)
		{
			if (this.RandomCarChances.TryGetValue(carName, out float weight))
			{
				return weight;
			}
			return 0f;
		}

		public void SetCarTypeChance(string carName, float weight, bool autoSave = true)
		{
			var randomCarChances = this.RandomCarChances;
			if (!randomCarChances.TryGetValue(carName, out float oldWeight) || oldWeight != weight)
			{
				randomCarChances[carName] = weight;
				if (autoSave)
				{
					this.Save();
				}
			}
		}

		public bool IsCarRandomnessEnabled(bool isOnline, bool isCarRival, string carName)
		{
			if (this.EnableRandomizedCars)
			{
				if (isOnline && this.RespectBackerCars && RandomCarType.IsBackerCar(carName))
				{
					// Only count setting when used for online cars (we still want backers to be able to randomize their own local replays).
					return false;
				}
				else if (isCarRival)
				{
					return this.RandomRivalCars;
				}
				else if (isOnline)
				{
					return this.RandomOnlineCars;
				}
				else
				{
					return this.RandomOfflineCars;
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

		private System.Random GetRandom(int seed, int placement, bool byPlacement, int skipCount)
		{
			if (byPlacement)
			{
				seed = placement;
			}
			else if (!this.FixedRandomness)
			{
				// Default constructor seed for System.Random, combined with an increment
				//  to ensure we're not creating multiple RNGs on the same tick.
				seed = unchecked(Environment.TickCount + this.unfixedRandomCount++);
			}
			System.Random rng = new System.Random(seed ^ unchecked((int)this.ExtraRandomnessSeed));

			for (int i = 0; i < skipCount; i++)
			{
				rng.NextDouble();
			}
			return rng;
		}

		public CarReplayData.CarData ChooseRandomCarData(CarReplayData.CarData origCarData, int seed, int placement,
														 List<RandomCarType> carTypes, List<RandomColorPreset> colorPresets)
		{
			var carRng = this.GetRandom(seed, placement, this.RandomCarByPlacement, 0);
			var colorRng = this.GetRandom(seed, placement, this.RandomColorByPlacement, 1);

			// ==== Choose our car type ====

			if (!RandomCarType.TryCreate(origCarData.name_, 1f, 1, out var carType))
			{
				carType = RandomCarType.DefaultCarType;
			}

			var carMethod = this.RandomCarMethod;
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
			var colorMethod = this.RandomColorMethod;
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

			if (this.RandomColorByPlacement)
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

			foreach (var carWeightPair in this.RandomCarChances)
			{
				if (RandomCarType.TryCreate(carWeightPair.Key, carWeightPair.Value, 1, out RandomCarType carType))
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

			float implicitCustomWeight = this.RandomCustomCarsChance;
			if (implicitCustomCount > 0 && implicitCustomWeight > 0f)
			{
				if (!this.IndividualRandomCustomCarsChance)
				{
					// The chance of choosing a custom car is split up equally across all custom cars.
					implicitCustomWeight /= implicitCustomCount;
				}

				foreach (string customCarName in RandomCarType.CustomCarNames)
				{
					if (!randomCarTypes.ContainsKey(customCarName) &&
						RandomCarType.TryCreate(customCarName, implicitCustomWeight, 1, out RandomCarType carType))
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

			/*if (!carTypesListedOnce)
			{
				carTypesListedOnce = true;
				Mod.Instance.Logger.Debug("====Known Cars====");
				int ii = 0;
				foreach (var knownCarPair in RandomCarType.KnownCars)
				{
					Mod.Instance.Logger.Debug($"knownCars[{ii}] = \"{knownCarPair.Key}\", index = {knownCarPair.Value}");
					ii++;
				}
				Mod.Instance.Logger.Debug("====Config Cars====");
				for (int i = 0; i < randomCarTypeList.Count; i++)
				{
					Mod.Instance.Logger.Debug($"randomCarTypeList[{i}] = \"{randomCarTypeList[i].Name}\", weight = {randomCarTypeList[i].Weight}");
				}
			}*/

			return randomCarTypeList;
		}

		//private static bool carTypesListedOnce = false;

		#endregion

		internal Settings Config;

		public event Action<ConfigurationLogic> OnChanged;

		private void Load()
		{
			Config = new Settings("Config");// Mod.FullName);
		}

		public void Awake()
		{
			Load();

			// Assign default settings (if not already assigned).
			Get(EnableSeparateMaxForSelectedReplays_ID, false);
			Get(MaxSelectedReplays_ID, 20);
			Get(FillWithLocalReplays_ID, false);

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
			Get(ShowDataEffectInGhostMode_ID, false);

			// Experimental (disabled by default)
			Get(EnableSteamRivals_ID, false);
			Convert(SteamRivals_ID, new Dictionary<ulong, string>(), overwriteNull: true);
			Get(HighlightRivalsInLeaderboards_ID, true);
			Get(UseRivalStyleForGhosts_ID, true);
			Get(UseRivalStyleForReplays_ID, false);
			Get(UseRivalStyleForSelf_ID, false);
			Get(RivalBrightness_ID, 1.0f);
			Get(RivalOutline_ID, true);
			Get(RivalDetailType_ID, CarLevelOfDetail.Type.Networked);

			// Randomized Cars
			Get(EnableRandomizedCars_ID, false);
			Get(FixedRandomness_ID, true);
			Get(ExtraRandomnessSeed_ID, (uint)0u);
			Get(RandomOfflineCars_ID, true);
			Get(RandomOnlineCars_ID, false);
			Get(RandomRivalCars_ID, false);
			Get(RespectBackerCars_ID, true);

			Convert(RandomCarChances_ID, RandomCarType.VanillaCarChances, overwriteNull: true);
			Get(RandomCustomCarsChance_ID, 0.0f);
			Get(IndividualRandomCustomCarsChance_ID, true);

			Get(RandomCarMethod_ID, RandomCarMethod.Car_Types);
			Get(RandomColorMethod_ID, RandomColorMethod.Color_Presets);
			Get(RandomCarByPlacement_ID, false);
			Get(RandomColorByPlacement_ID, false);

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
