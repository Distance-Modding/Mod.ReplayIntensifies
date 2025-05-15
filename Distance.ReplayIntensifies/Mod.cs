using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Distance.ReplayIntensifies.Data;
using Distance.ReplayIntensifies.Helpers;
using Distance.ReplayIntensifies.Randomizer;
using Distance.ReplayIntensifies.Scripts;
using HarmonyLib;
using JsonFx.Json;
using JsonFx.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Distance.ReplayIntensifies
{
	/// <summary>
	/// The mod's main class containing its entry point
	/// </summary>
	[BepInPlugin(modGUID, modName, modVersion)]
	public sealed class Mod : BaseUnityPlugin
	{
		//Mod Details
		private const string modGUID = "Distance.ReplayIntensifies";
		private const string modName = "Replay Intensifies";
		private const string modVersion = "1.3.2";

		//Config Entries
		public static ConfigEntry<bool> EnableSeparateMaxForSelectedReplays { get; set; }
		public static ConfigEntry<int> MaxAutoReplays { get; set; }
		public static ConfigEntry<int> MaxSelectedReplays { get; set; }
		public static ConfigEntry<bool> FillWithLocalReplays { get; set; }
		public static ConfigEntry<LocalLeaderboardTrimming> LocalReplayTrimming { get; set; }
		public static ConfigEntry<int> MaxSavedLocalReplays { get; set; }
		public static ConfigEntry<int> MaxOnlineLeaderboards { get; set; }
		public static ConfigEntry<bool> GhostOutline { get; set; }
		public static ConfigEntry<CarLevelOfDetail.Type> GhostDetailType { get; set; }
		public static ConfigEntry<bool> ReplayOutline { get; set; }
		public static ConfigEntry<CarLevelOfDetail.Type> ReplayDetailType { get; set; }
		public static ConfigEntry<CarLevelOfDetail.Level> MaxLevelOfDetail {
			get
			{
				var value = MaxLevelOfDetail.Value;
				if (value < MaxMaxLevelOfDetail) value = MaxMaxLevelOfDetail;
				if (value > MinMaxLevelOfDetail) value = MinMaxLevelOfDetail;
				MaxLevelOfDetail.Value = value;
				return MaxLevelOfDetail;
			}
			set
			{
				if (value.Value < MaxMaxLevelOfDetail) value.Value = MaxMaxLevelOfDetail;
				if (value.Value > MinMaxLevelOfDetail) value.Value = MinMaxLevelOfDetail;
				MaxLevelOfDetailCached = value.Value;
			}
		}
		public static ConfigEntry<CarLevelOfDetail.Level> MinLevelOfDetail {
			get
			{
				var value = MinLevelOfDetail.Value;
				if (value < MaxMinLevelOfDetail) value = MaxMinLevelOfDetail;
				MinLevelOfDetail.Value = value;
				return MinLevelOfDetail;
			}
			set
			{
				if (value.Value < MaxMinLevelOfDetail) value.Value = MaxMinLevelOfDetail;
				MinLevelOfDetailCached = value.Value;
			}
		}
		public static ConfigEntry<bool> EnableUnrestrictedOpponentColors { get; set; }
		public static ConfigEntry<GhostOrReplay> UseDataEffectForMode { get; set; }
		public static ConfigEntry<GhostOrReplay> UseRivalStyleForMode { get; set; }
		public static ConfigEntry<bool> UseRivalStyleForSelf { get; set; }
		public static ConfigEntry<float> RivalBrightness { get; set; }
		public static ConfigEntry<bool> RivalOutline { get; set; }
		public static ConfigEntry<CarLevelOfDetail.Type> RivalDetailType { get; set; }
		public static ConfigEntry<bool> EnableSteamRivals { get; set; }
		public static ConfigEntry<bool> HighlightRivalsInLeaderboards { get; set; }
		public static ConfigEntry<bool> EnableRandomizedCars { get; set; }
		public static ConfigEntry<uint> ExtraRandomnessSeed { get; set; }
		public static ConfigEntry<LocalOrOnline> UseRandomCarsFor { get; set; }
		public static ConfigEntry<bool> UseRandomRivalCars { get; set; }
		public static ConfigEntry<bool> RandomRespectBackerCars { get; set; }
		public static ConfigEntry<RandomSeedMethod> RandomCarSeedMethod { get; set; }
		public static ConfigEntry<RandomSeedMethod> RandomColorSeedMethod { get; set; }
		public static ConfigEntry<RandomCarMethod> RandomCarChoiceMethod { get; set; }
		public static ConfigEntry<RandomColorMethod> RandomColorChoiceMethod { get; set; }
		public static ConfigEntry<bool> RandomRequireCarUnlocks { get; set; }
		public static ConfigEntry<float> RandomCustomCarsDefaultWeight { get; set; }
		public static ConfigEntry<bool> RandomCustomCarsSplitDefaultWeight { get; set; }
		public static ConfigEntry<bool> ReplayModeDisableCinematicCameras { get; set; }
		public static ConfigEntry<bool> ReplayModePauseAtStart { get; set; }
		public static ConfigEntry<float> FinishPreSpectateTime { get; set; }

		//Const
		public const int OriginalMaxReplays = 20;
		public const int OriginalMaxSavedLocalReplays = 20;
		public const int OriginalMaxOnlineLeaderboards = 1000;

		public const int MaxReplaysAtAll = 1000;
		public const int MaxSavedLocalReplaysAtAll = 10000;
		public const int MaxOnlineLeaderboardsAtAll = 10000;

		public const int MinOnlineLeaderboards = 15;

		public const float MaxRandomWeight = 100f;

		public const bool Debug_TestBackerCars = false;

		// In-Focus LODs are only used for the replay car with camera focus.
		// Unfocused cars should never have an LOD higher than Near. And having a lower max LOD is only
		//  necessary for unfocused cars, so remove these options and exclude both In-Focus's from Max LOD checks.
		public const CarLevelOfDetail.Level MaxMaxLevelOfDetail = CarLevelOfDetail.Level.Near; // Below In-Focus.
		public const CarLevelOfDetail.Level MinMaxLevelOfDetail = CarLevelOfDetail.Level.Speck; // or VeryFar
		public const CarLevelOfDetail.Level MaxMinLevelOfDetail = CarLevelOfDetail.Level.InFocus; // Allow forcing up to In-Focus

		//Public Variables
		public static Dictionary<ulong, string> SteamRivals { get; set; }
		public static Dictionary<string, float> RandomCarWeights { get; set; }


		// Cached property values for faster accessing.
		public static CarLevelOfDetail.Level MaxLevelOfDetailCached { get; private set; }
		public static CarLevelOfDetail.Level MinLevelOfDetailCached { get; private set; }

		//Other
		private static readonly Harmony harmony = new Harmony(modGUID);
		public static ManualLogSource Log = new ManualLogSource(modName);
		public static Mod Instance;

		/// <summary>
		/// Method called as soon as the mod is loaded.
		/// WARNING:	Do not load asset bundles/textures in this function
		///				The unity assets systems are not yet loaded when this
		///				function is called. Loading assets here can lead to
		///				unpredictable behaviour and crashes!
		/// </summary>
		public void Awake()
		{
			// Do not destroy the current game object when loading a new scene
			DontDestroyOnLoad(this);

			if (Instance == null)
			{
				Instance = this;
			}

			Log = BepInEx.Logging.Logger.CreateLogSource(modGUID);


			try
			{
				SteamworksHelper.Init(); // Handle this here for early error reporting.
			}
			catch (Exception ex)
			{
				Log.LogError(modName + ": Error during SteamworksHelper.Init()");
				Log.LogError(ex);
				throw;
			}

			

			UseDataEffectForMode = Config.Bind("Rendering Settings",
				"USE DATA EFFECT FOR MODE",
				GhostOrReplay.Replay_Mode,
				new ConfigDescription("Which modes to use the data materialization spawn/finish-despawn effect in for non-ghost car styles."));

			GhostDetailType = Config.Bind("Rendering Settings",
				"GHOST CAR STYLE",
				CarLevelOfDetail.Type.Ghost,
				new ConfigDescription("Change the visual detail type of Ghost cars."));

			GhostOutline = Config.Bind("Rendering Settings",
				"GHOST OUTLINE",
				true,
				new ConfigDescription("Whether or not Ghost cars have an outline"));

			ReplayDetailType = Config.Bind("Rendering Settings",
				"REPLAY CAR STYLE",
				CarLevelOfDetail.Type.Replay,
				new ConfigDescription("Change the visual detail type of Replay Mode cars."));

			ReplayOutline = Config.Bind("Rendering Settings",
				"REPLAY OUTLINE",
				false,
				new ConfigDescription("Whether or not Replay Mode cars have an outline"));

			MaxLevelOfDetail = Config.Bind("Rendering Settings",
				"MAX CAR LEVEL OF DETAIL",
				MaxMaxLevelOfDetail,
				new ConfigDescription("Change the highest level of detail that opponent cars will render with." +
				" Lowering Max Level of Detail can improve performance when playing with more ghosts."));

			MinLevelOfDetail = Config.Bind("Rendering Settings",
				"MIN CAR LEVEL OF DETAIL",
				CarLevelOfDetail.Level.Speck,
				new ConfigDescription("Change the lowest level of detail that opponent cars will render with." +
				" Raising Min Level of Detail can decrease performance when playing with more ghosts." +
				" NOTE: In-Focus will force a car's LOD to be higher than normal for non-camera-focused cars."));
			
			EnableSeparateMaxForSelectedReplays = Config.Bind("Limits Settings",
				"USE MAX SELECTED REPLAYS",
				false,
				new ConfigDescription("Use a separate maximum for the number of selected ghosts from the leaderboards menu."));

			MaxAutoReplays = Config.Bind("Limits Settings",
				"MAX AUTO REPLAYS",
				5,
				new ConfigDescription("Maximum number of ghosts that will auto-load when playing a level. This is the [i]GHOSTS IN ARCADE COUNT[/i] option from the Replays menu, and is included here for convenience.",
					new AcceptableValueRange<int>(1, MaxReplaysAtAll)));

			MaxSelectedReplays = Config.Bind("Limits Settings",
				"MAX SELECTED REPLAYS",
				20,
				new ConfigDescription("Maximum number of ghosts that will be loaded when selecting from the leaderboards menu.",
					new AcceptableValueRange<int>(OriginalMaxReplays, MaxReplaysAtAll)));

			MaxOnlineLeaderboards = Config.Bind("Limits Settings",
				"MAX ONLINE LEADERBOARD RANKS",
				1000,
				new ConfigDescription("Maximum number of leaderboard ranks shown for Friends and Online tabs.",
					new AcceptableValueRange<int>(MinOnlineLeaderboards, MaxOnlineLeaderboardsAtAll)));

			MaxSavedLocalReplays = Config.Bind("Limits Settings",
				"MAX SAVED LOCAL REPLAYS",
				500,
				new ConfigDescription("Maximum number of local leaderboard replays that will be saved.\n[FF0000]WARNING:[-] Completing a map with more than this number of ghosts will remove ALL ghosts past the maximum.",
					new AcceptableValueRange<int>(OriginalMaxSavedLocalReplays, MaxSavedLocalReplaysAtAll)));

			LocalReplayTrimming = Config.Bind("Limits Settings",
				"LOCAL REPLAY TRIMMING",
				LocalLeaderboardTrimming.Current,
				new ConfigDescription("When creating a local leaderboard replay past the placement limit, choose whether replays are Never deleted, only your Current Run gets deleted, or if all replays past the limit Always get deleted."));

			FillWithLocalReplays = Config.Bind("Limits Settings",
				"FILL WITH LOCAL REPLAYS",
				false,
				new ConfigDescription("Fill remaining auto slots with local replays when there aren't enough online replays to load."));

			EnableUnrestrictedOpponentColors = Config.Bind("Limits Settings",
				"ENABLE UNRESTRICTED OPPONENT COLORS",
				false,
				new ConfigDescription("Online opponents and non-[i]Ghost Detail Type[/i] cars will NOT have their colors clamped, allowing for extremely bright cars." +
				" Bright cars are made by editing color preset files and changing the color channels to very large values."));

			ReplayModeDisableCinematicCameras = Config.Bind("Replay Mode Settings",
				"DISABLE CINEMATIC CAMERAS",
				false,
				new ConfigDescription("Turn off cinematic camera triggers in replay mode, allowing for more flexible camera movement."));

			ReplayModePauseAtStart = Config.Bind("Replay Mode Settings",
				"PAUSE AT START",
				false,
				new ConfigDescription("Pause at match start during replay mode, allowing easier tracking of extremely short runs."));

			FinishPreSpectateTime = Config.Bind("Replay Mode Settings",
				"FINISH PRE-SPECTATE TIME",
				5f,
				new ConfigDescription("Change the number of seconds to wait after finishing a level before going into spectate mode.",
					new AcceptableValueRange<float>(0.02f, 300f)));

			SteamRivals = LoadSteamRivals();

			EnableSteamRivals = Config.Bind("Steam Rival Settings",
				"ENABLE STEAM RIVALS",
				false,
				new ConfigDescription("Enable the Steam Rivals feature."));

			HighlightRivalsInLeaderboards = Config.Bind("Steam Rival Settings",
				"HIGHLIGHT RIVALS IN LEADERBOARDS",
				false,
				new ConfigDescription("Steam Rivals listed in the level select leaderboards menu will be colored differently."));

			UseRivalStyleForMode = Config.Bind("Steam Rival Settings",
				"USE CAR STYLE FOR MODE",
				GhostOrReplay.Ghost_Mode,
				new ConfigDescription("Which modes to use the Steam Rival car style in."));

			UseRivalStyleForSelf = Config.Bind("Steam Rival Settings",
				"USE CAR STYLE FOR SELF",
				false,
				new ConfigDescription("Steam Rival car styles will also be used for your own ghosts."));

			RivalDetailType = Config.Bind("Steam Rival Settings",
				"RIVAL CAR STYLE",
				CarLevelOfDetail.Type.Networked,
				new ConfigDescription("Change the visual detail type of Steam Rival cars."));

			RivalOutline = Config.Bind("Steam Rival Settings",
				"RIVAL CAR OUTLINE",
				true,
				new ConfigDescription("Change whether the Steam Rival has an outline"));

			RivalBrightness = Config.Bind("Steam Rival Settings",
				"RIVAL OUTLINE BRIGHTNESS",
				1.0f,
				new ConfigDescription("Change the brightness for Steam Rival car outlines." +
				" NOTE: Brightness values higher than 1.0 will only increase the intensity of flames and wing trails.",
					new AcceptableValueRange<float>(0.05f, 10f)));

			RandomCarWeights = LoadCarWeights();

			EnableRandomizedCars = Config.Bind("Randomized Cars Settings",
				"ENABLE RANDOMIZED CARS",
				false,
				new ConfigDescription("Enable the randomized cars feature, allowing for more variety with opponents."));

			UseRandomCarsFor = Config.Bind("Randomized Cars Settings",
				"USE RANDOM CARS FOR",
				LocalOrOnline.Local_Replays,
				new ConfigDescription("Choose whether local and/or online leaderboards replays will be randomized." +
				" 'Online' is not recommended for normal play, since a player's car can be considered part of 'their identity' in the leaderboards."));

			UseRandomRivalCars = Config.Bind("Randomized Cars Settings",
				"USE RANDOM RIVAL CARS",
				false,
				new ConfigDescription("Rival cars will be randomized (STEAM RIVALS feature must be enabled)."));

			RandomRespectBackerCars = Config.Bind("Randomized Cars Settings",
				"RESPECT KICKSTARTER BACKERS",
				true,
				new ConfigDescription("Disable randomizing online replays that use the Kickstarter backer car."));

			ExtraRandomnessSeed = Config.Bind("Randomized Cars Settings",
				"EXTRA RANDOMNESS SEED",
				0u,
				new ConfigDescription("Change up the fixed randomness for replays a little.",
					new AcceptableValueRange<uint>(0u, 10u)));

			/*randomSubmenu.InputPrompt(MenuDisplayMode.Both,
				"setting:extra_randomness",
				"EXTRA RANDOMNESS SEED",
				OnSubmitExtraRandomnessSeed,
				null,
				OnValidateExtraRandomnessSeed,
				"ENTER AN INTEGER",
				null,
				"Change up the fixed randomness for replays a little." +
				$" Enter a number between 0 and {uint.MaxValue} (0x{uint.MaxValue:X8}). Or enter " + RngKeyword + " to generate a random number.")
				.WithDefaultValue(() => Config.ExtraRandomnessSeed.ToString());*/

			const string SeedMethodDescription =
				" By Replay will use the replay data as the seed, so that each replay will always be the same." +
				" By Placement will use the placement between all replays as the seed.";

			const string ChoiceMethodDescription =
				" Cycle will randomly cycle through choices once before choosing duplicates.";

			RandomCarSeedMethod = Config.Bind("Randomized Car Settings",
				"CAR SEED METHOD",
				RandomSeedMethod.By_Replay,
				new ConfigDescription("Change how randomness is determined for car types." + SeedMethodDescription));

			RandomColorSeedMethod = Config.Bind("Randomized Car Settings",
				"COLOR SEED METHOD",
				RandomSeedMethod.By_Replay,
				new ConfigDescription("Change how randomness is determined for car colors." + SeedMethodDescription));

			RandomCarChoiceMethod = Config.Bind("Randomized Car Settings",
				"CAR CHOICE METHOD",
				RandomCarMethod.Car_Types,
                new ConfigDescription("Choose how random car types will be decided." + ChoiceMethodDescription));

			RandomColorChoiceMethod = Config.Bind("Randomized Car Settings",
				"COLOR CHOICE METHOD",
				RandomColorMethod.Color_Presets,
				new ConfigDescription("Choose how random car colors will be decided." + ChoiceMethodDescription));

			RandomCustomCarsDefaultWeight = Config.Bind("Randomized Car Settings",
				"DEFAULT CUSTOM CARS WEIGHT",
				0.0f,
				new ConfigDescription("Default weighted chance for any custom car with an individual weight of 0.0." +
				" Use this when you want all custom cars to have a chance of appearing, without having to touch the settings after new additions (0.0 to disable).",
					new AcceptableValueRange<float>(0.0f, MaxRandomWeight)));

			RandomCustomCarsSplitDefaultWeight = Config.Bind("Randomized Car Settings",
				"SPLIT DEFAULT CUSTOM CARS WEIGHT",
				false,
				new ConfigDescription("Any custom car using the default weight will split the weighted chance of appearing among all custom cars, rather than being given the same weight (see option above)."));

			RandomRequireCarUnlocks = Config.Bind("Randomized Car Settings",
				"REQUIRE CAR UNLOCKS",
				false,
				new ConfigDescription("Disable using random car types that you haven't unlocked yet."));

			MaxAutoReplays.SettingChanged += OnConfigChanged;

			Log.LogInfo(modName + ": Initializing...");
			harmony.PatchAll();
			Log.LogInfo(modName + ": Initialized!");
		}

		private void OnConfigChanged(object sender, EventArgs e)
		{
			SettingChangedEventArgs settingChangedEventArgs = e as SettingChangedEventArgs;

			if(sender == MaxAutoReplays)
            {
				G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_ = (int)settingChangedEventArgs.ChangedSetting.BoxedValue;
				G.Sys.OptionsManager_.Replay_.Save();
				return;
            }

			if(sender == RivalBrightness)
            {
				// Treat changes to this value as an event since we don't have many other options for detecting if our own menu is closed.
				//  (GSL never broadcasts the Events.GUI.MenuClosed static event)
				// It's important to broadcast this so that outline brightnesses can be updated mid-game.
				Events.ReplayOptionsMenu.MenuClose.Broadcast(null);
				return;
			}

			if(sender == ExtraRandomnessSeed)
            {

            }

			if (settingChangedEventArgs == null) return;
		}

		public void SaveDictionary(Dictionary<string, float> dic)
        {
			string fileName = "Car Weights.json";
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
			DataWriterSettings st = new DataWriterSettings { PrettyPrint = true };
			JsonWriter writer = new JsonWriter(st);
			try
			{
				using (var sw = new StreamWriter(Path.Combine(rootDirectory, fileName), false))
				{
					sw.WriteLine(writer.Write(dic));
				}
			}
			catch (Exception e)
			{
				Log.LogWarning(e);
			}
		}

		public void SaveDictionary(Dictionary<ulong, string> dic)
		{
			string fileName = "Steam Rivals.json";
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
			DataWriterSettings st = new DataWriterSettings { PrettyPrint = true };
			JsonWriter writer = new JsonWriter(st);
			try
			{
				using (var sw = new StreamWriter(Path.Combine(rootDirectory, fileName), false))
				{
					sw.WriteLine(writer.Write(dic));
				}
			}
			catch (Exception e)
			{
				Log.LogWarning(e);
			}
		}

		private Dictionary<string, float> LoadCarWeights()
        {
			string fileName = "Car Weights.json";
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

			try
            {
				using (var sr = new StreamReader(Path.Combine(rootDirectory, fileName)))
                {
					string json = sr.ReadToEnd();
					JsonReader reader = new JsonReader();
					Dictionary<string, float> WeightDictionary = reader.Read<Dictionary<string, float>>(json);

					return WeightDictionary;
				}
            }
			catch(DirectoryNotFoundException ex)
            {
				Log.LogWarning("Failed to load car randomization weights due to the directory not existing. \nNew weights will be saved when necessary.");
				return new Dictionary<string, float>();
			}
			catch(Exception ex)
            {
				Log.LogWarning("Failed to load car randomization weights");
				Log.LogWarning(ex);
				return new Dictionary<string, float>();
			}
        }

		private Dictionary<ulong, string> LoadSteamRivals()
        {
			string fileName = "Steam Rivals.json";
			string rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);

			try
			{
				using (var sr = new StreamReader(Path.Combine(rootDirectory, fileName)))
				{
					string json = sr.ReadToEnd();
					JsonReader reader = new JsonReader();
					Dictionary<string, string> SteamRivalDictionary = reader.Read<Dictionary<string, string>>(json);

					Dictionary<ulong, string> CorrectedDictionary = new Dictionary<ulong, string>();

					foreach(KeyValuePair<string, string> kvPair in SteamRivalDictionary)
                    {
						CorrectedDictionary.Add(ulong.Parse(kvPair.Key), kvPair.Value);
                    }

					return CorrectedDictionary;
				}
			}
			catch (DirectoryNotFoundException ex)
			{
				Log.LogWarning("Failed to load Steam Rivals due to the directory not existing. \nNew Rivals will be saved when necessary.");
				return new Dictionary<ulong, string>();
			}
			catch (Exception ex)
			{
				Log.LogWarning("Failed to load Steam Rivals");
				Log.LogWarning("Exception Name: " + ex.GetType().ToString());
				Log.LogWarning(ex);
				return new Dictionary<ulong, string>();
			}
        }

		#region Settings Helpers

		private static Dictionary<string, CarLevelOfDetail.Level> GetLevelOfDetailSettingsEntries()
		{
			/*return new Dictionary<string, CarLevelOfDetail.Level>
			{
				{ "In-Focus (First Person)", CarLevelOfDetail.Level.InFocusFP }, // 0
				{ "In-Focus",                CarLevelOfDetail.Level.InFocus },   // 1
				{ "Near",                    CarLevelOfDetail.Level.Near },      // 2
				{ "Medium",                  CarLevelOfDetail.Level.Medium },    // 3
				{ "Far",                     CarLevelOfDetail.Level.Far },       // 4
				{ "Very Far",                CarLevelOfDetail.Level.VeryFar },   // 5
				{ "Speck",                   CarLevelOfDetail.Level.Speck },     // 6
			};*/

			return new Dictionary<string, CarLevelOfDetail.Level>
			{
				{ "Very Low",                CarLevelOfDetail.Level.Speck },     // 6
				{ "Low",                     CarLevelOfDetail.Level.VeryFar },   // 5
				{ "Medium",                  CarLevelOfDetail.Level.Far },       // 4
				{ "High",                    CarLevelOfDetail.Level.Medium },    // 3
				{ "Ultra",                   CarLevelOfDetail.Level.Near },      // 2
				{ "Ultra (In-Focus)",        CarLevelOfDetail.Level.InFocus },   // 1
				{ "Ultra (First Person)",    CarLevelOfDetail.Level.InFocusFP }, // 0
			};
		}

		private static Dictionary<string, CarLevelOfDetail.Level> GetMaxLevelOfDetailSettingsEntries()
		{
			var maxDetailLevelEntries = GetLevelOfDetailSettingsEntries();
			// Remove unsupported LODs from max entries.
			foreach (var lodPair in maxDetailLevelEntries.ToArray()) // ToArray so we can modify the dictionary during foreach.
			{
				if (lodPair.Value < Mod.MaxMaxLevelOfDetail || lodPair.Value > Mod.MinMaxLevelOfDetail)
				{
					maxDetailLevelEntries.Remove(lodPair.Key);
				}
			}
			return maxDetailLevelEntries;
		}

		private static Dictionary<string, CarLevelOfDetail.Level> GetMinLevelOfDetailSettingsEntries()
		{
			var minDetailLevelEntries = GetLevelOfDetailSettingsEntries();
			// Remove unsupported LODs from min entries.
			foreach (var lodPair in minDetailLevelEntries.ToArray()) // ToArray so we can modify the dictionary during foreach.
			{
				if (lodPair.Value < Mod.MaxMinLevelOfDetail)
				{
					minDetailLevelEntries.Remove(lodPair.Key);
				}
			}
			return minDetailLevelEntries;
		}

		#endregion

		#region Settings Menu Events

		private string OnValidateFloat(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return null;
			}
			else if (!float.TryParse(input, out float result) || float.IsNaN(result) || float.IsInfinity(result))
			{
				return "This is not a valid decimal number";
			}
			return null;
		}

		private string OnValidateFloatOrRng(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return null;
			}
			else if (!float.TryParse(input, out float result) || float.IsNaN(result) || float.IsInfinity(result))
			{
				return "This is not a valid decimal number or ";
			}
			return null;
		}

		private void OnSubmitRivalOutlineBrightness(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				// Don't change
			}
			else if (float.TryParse(input, out float result) && !float.IsNaN(result) && !float.IsInfinity(result))
			{
				// Allow extremely high outline brightness (which doesn't affect outline itself, but does affect jets and wings).
				// Anything higher than 100,000 will produce black splotches from the intensity.
				result = Mathf.Clamp(result, 0.05f, 100_000f);
				if (RivalBrightness.Value != result)
				{
					RivalBrightness.Value = result;

					// Treat changes to this value as an event since we don't have many other options for detecting if our own menu is closed.
					//  (GSL never broadcasts the Events.GUI.MenuClosed static event)
					// It's important to broadcast this so that outline brightnesses can be updated mid-game.
					Events.ReplayOptionsMenu.MenuClose.Broadcast(null);
				}
			}
		}

		private string OnValidateExtraRandomnessSeed(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return null;
			}
			else if (!uint.TryParse(input, out _))
			{
				return $"Not a valid integer between 0 and {uint.MaxValue} or ";
			}
			return null;
		}

		private void OnSubmitExtraRandomnessSeed(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				// Don't change
			}
			else if (uint.TryParse(input, NumberStyles.AllowHexSpecifier, null, out uint result))
			{
				ExtraRandomnessSeed.Value = result;
			}
		}

		private static bool FilterCarType(string carName, string filter)
		{
			if (!string.IsNullOrEmpty(filter))
			{
				// Use IndexOf because there's no override for Contains with StringComparison.
				return (carName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1);
			}
			return true;
		}

		private void SetRandomCarWeights(IEnumerable<string> carNames, string filter)
		{
			var rng = new System.Random();
			foreach (string carName in carNames.Where(c => FilterCarType(c, filter)))
			{
				SetCarTypeChance(carName, (float)rng.NextDouble(), false);
			}
		}

		public void SetCarWeights(IEnumerable<string> carNames, string filter, float weight)
		{
			Log.LogWarning("Setting Car Weights");
			weight = Mathf.Clamp(weight, 0.0f, MaxRandomWeight);
			foreach (string carName in carNames.Where(c => FilterCarType(c, filter)))
			{
				Log.LogInfo("Car weight for: " + carName);
				SetCarTypeChance(carName, weight);
			}
		}

		private void OnSubmitSetVanillaCarWeights(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				// Don't change
			}
			else if (float.TryParse(input, out float result) && !float.IsNaN(result) && !float.IsInfinity(result))
			{
				SetCarWeights(RandomCarType.AllowedVanillaCarNames, null, result);
			}
		}

		private void OnSubmitSetCustomCarWeights(string input, string filter)
		{
			if (string.IsNullOrEmpty(input))
			{
				// Don't change
			}
			else if (float.TryParse(input, out float result) && !float.IsNaN(result) && !float.IsInfinity(result))
			{
				SetCarWeights(RandomCarType.CustomCarNames, filter, result);
			}
		}

		private void OnSubmitSetCustomCarWeightsWithFilter(string filter)
		{
			// NOTE: It's impossible to open an InputPromptPanel on the same frame as its onSubmit or onPop actions.
			//       This is because the top menu panel will get popped by the first InputPromptPanel when closing,
			//       but what will be popped is our nested InputPromptPanel that we just attempted to create.
			//       To solve this, simply wait one frame before creating our 'nested' InputPromptPanel.
			// tl;dr: Do this next frame so that our previous InputPromptPanel can properly pop before we create our next one.
			this.DoNextFrame(() => {

				// Nested prompt to actually ask for the new weight.
				InputPromptPanel.Create(
					(out string error, string input) => {

						error = OnValidateFloatOrRng(input);

						if (error == null)
						{
							OnSubmitSetCustomCarWeights(input, filter);
							return true;
						}
						return false;
					},
					null,
					"ENTER CUSTOM WEIGHT",
					null);
			});
		}

		private void OnConfirmResetVanillaCarWeights()
		{
			var defaultValues = GetDefaultCarWeights();
			foreach (string carName in RandomCarType.AllowedVanillaCarNames)
			{
				defaultValues.TryGetValue(carName, out float defaultValue); // Default weight or 0f.

				SetCarTypeChance(carName, defaultValue, false); // Hold off on auto-saving until afterwards.
			}
		}

		#endregion

		#region Transpiler Helper Methods

		/// <summary>
		/// Gets the max number of replays that will automatically download when selecting a level.
		/// </summary>
		/// <remarks>
		/// This exists to inline a comparison operand.
		/// </remarks>
		public static int GetMaxAutoReplays()
		{
			return Mathf.Clamp(G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_, Mod.OriginalMaxReplays, Mod.MaxReplaysAtAll);
		}

		/// <summary>
		/// Gets the max number of replays that will play when selected from the advanced leaderboards menu.
		/// </summary>
		/// <remarks>
		/// This exists to inline a comparison operand.
		/// </remarks>
		public static int GetMaxPickedReplays(/*bool replayMode*/)
		{
			if (EnableSeparateMaxForSelectedReplays.Value)
			{
				return Mathf.Clamp(Mod.MaxSelectedReplays.Value, Mod.OriginalMaxReplays, Mod.MaxReplaysAtAll);
			}
			else
			{
				return Mod.GetMaxAutoReplays();
			}
		}

		/// <summary>
		/// Gets the max number of replays from auto downloading or manually selecting.
		/// </summary>
		/// <remarks>
		/// This exists to inline a comparison operand.
		/// </remarks>
		public static int GetMaxSpawnReplays()
		{
			return Math.Max(Mod.GetMaxAutoReplays(), Mod.GetMaxPickedReplays());
		}

		/// <summary>
		/// Gets the max number of replays that can be stored in Local leaderboards.
		/// </summary>
		/// <remarks>
		/// This exists to inline a comparison operand.
		/// </remarks>
		public static int GetMaxSavedLocalReplays()
		{
			return Mathf.Clamp(MaxSavedLocalReplays.Value, OriginalMaxSavedLocalReplays, MaxSavedLocalReplaysAtAll);
		}

		/// <summary>
		/// Gets the max number of entries that will show up in Friends/Global leaderboards.
		/// </summary>
		/// <remarks>
		/// This exists to inline a passed argument operand.
		/// </remarks>
		public static int GetMaxOnlineLeaderboards()
		{
			return Mathf.Clamp(MaxOnlineLeaderboards.Value, MinOnlineLeaderboards, MaxOnlineLeaderboardsAtAll);
		}

		/// <summary>
		/// Handles deciding whether car colors can be clamped or not.
		/// </summary>
		/// <remarks>
		/// This exists because it was difficult to correctly patch the <see cref="PlayerDataBase.GetClampCarColors"/>
		/// property.
		/// </remarks>
		public static bool GetClampCarColors(PlayerDataBase playerDataBase)
		{
			bool result = playerDataBase.ClampCarColors_;
			if (result && playerDataBase is PlayerDataOpponent)
			{
				//Mod.Instance.Logger.Debug("GetClampCarColors: playerDataBase is PlayerDataOpponent");
				return !EnableUnrestrictedOpponentColors.Value;
			}
			return result;
		}

		/// <summary>
		/// Handles situations where a replay car needs to know if its visually a ghost (rather than functionally).
		/// </summary>
		/// <remarks>
		/// This exists to avoid code duplication between multiple transpilers.
		/// </remarks>
		public static bool GetIsGhostVisual(PlayerDataReplay playerDataReplay)
		{
			var compoundData = playerDataReplay.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				return compoundData.IsGhostVisual;
			}
			return playerDataReplay.IsGhost_;
		}

		/// <summary>
		/// Handles updating a replay player's brightness to conform to this mod's settings.
		/// </summary>
		/// <remarks>
		/// This exists to avoid code duplication between multiple transpilers.
		/// </remarks>
		public static float GetGhostBrightness(PlayerDataReplay playerDataReplay)
		{
			var compoundData = playerDataReplay.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				return compoundData.GetOutlineBrightness();
			}
			return playerDataReplay.replaySettings_.GhostBrightness_;
		}

		/// <summary>
		/// Conditionally creates a car's outline based on the current settings.
		/// </summary>
		/// <remarks>
		/// This exists so that outlines can be disabled for all <see cref="CarLevelOfDetail.Type"/>'s without having
		/// to overwrite the <see cref="PlayerDataReplay.InitCarBlueprintVirtual(GameObject)"/> method.
		/// </remarks>
		public static void CreateCarOutline(PlayerDataReplay playerDataReplay)
		{
			var compoundData = playerDataReplay.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData.HasOutline)
			{
				playerDataReplay.CreateCarOutline();
			}
		}

		/// <summary>
		/// Determines if the data materialization effect should not be used when spawning a replay/ghost car.
		/// </summary>
		public static bool GetDontShowDataEffect(PlayerDataReplay playerDataReplay)
		{
			var compoundData = playerDataReplay.GetComponent<PlayerDataReplayCompoundData>();
			if (compoundData)
			{
				return !compoundData.ShowDataEffect;
			}
			return playerDataReplay.IsGhost_; // Default behavior.
		}

		/// <summary>
		/// Changes the leaderboard button color when using highlighting for Steam Rivals.
		/// </summary>
		/// <remarks>
		/// This exists because this functionality is needed in two different patches.
		/// </remarks>
		public static void UpdateLeaderboardButtonColor(LevelSelectLeaderboardButton button, bool force)
		{
			if (!EnableSteamRivals.Value || !HighlightRivalsInLeaderboards.Value)
			{
				return;
			}

			LevelSelectLeaderboardMenu.Entry entry = button.entry_ as LevelSelectLeaderboardMenu.Entry;

			Color color = (!entry.info_.isLocal_) ? Color.white : LevelSelectLeaderboardButton.localColor_;
			bool isRival = false;

			if (!entry.info_.isLocal_ && entry.leaderboardEntry_ is SteamworksLeaderboard.Entry steamEntry)
			{
				ulong steamID = SteamworksHelper.GetLeaderboardEntrySteamID(steamEntry);
				if (Instance.IsSteamRival(steamID, true))
				{
					color = ColorEx.HexToColor("9480E7"); // Same as sprint campaign level set color (might be a little dark)
					isRival = true;
				}
			}

			if (isRival || force)
			{
				button.SetLabelColor(button.nameLabel_, color);
				button.SetLabelColor(button.placeLabel_, color);
				button.SetLabelColor(button.dataLabel_, color);
			}
		}

		#endregion

		#region Other Helper Methods

		public CarLevelOfDetail.Type GetCarDetailType(bool isGhost, bool isCarRival)
		{
			if (isCarRival)
			{
				return RivalDetailType.Value;
			}
			return (isGhost) ? GhostDetailType.Value : ReplayDetailType.Value;
		}

		public bool GetCarOutline(bool isGhost, bool isCarRival)
		{
			if (isCarRival)
			{
				return RivalOutline.Value;
			}
			return (isGhost) ? GhostOutline.Value : ReplayOutline.Value;
		}

		// Determines if this car should be displayed as a Steam Rival (which accounts for settings like 'Use rival style for ghosts/replays', etc.).
		public bool IsCarSteamRival(bool isGhost, long userID) => IsCarSteamRival(isGhost, unchecked((ulong)userID));

		public bool IsCarSteamRival(bool isGhost, ulong userID)
		{
			if (EnableSteamRivals.Value && UseRivalStyleForMode.Value.HasGhostOrReplay(isGhost))
			{
				return IsSteamRival(userID, false);
			}
			return false;
		}

		public float GetCarTypeChance(string carName)
		{
			if (RandomCarWeights.TryGetValue(carName, out float weight))
			{
				return weight;
			}
			return 0f;
		}

		public void SetCarTypeChance(string carName, float weight, bool autoSave = true)
		{
			var randomCarChances = RandomCarWeights;
			if (!randomCarChances.TryGetValue(carName, out float oldWeight) || oldWeight != weight)
			{
				randomCarChances[carName] = weight;

				if (autoSave)
				{
					SaveDictionary(RandomCarWeights);
				}
			}
		}

		public bool IsCarRandomnessEnabled(bool isOnline, bool isCarRival, string carName)
		{
			if (EnableRandomizedCars.Value)
			{
				if (isOnline && RandomRespectBackerCars.Value && RandomCarType.IsBackerCar(carName))
				{
					// Only count setting when used for online cars (we still want backers to be able to randomize their own local replays).
					return false;
				}
				else if (isCarRival)
				{
					return UseRandomRivalCars.Value;
				}
				else if (isOnline)
				{
					return UseRandomCarsFor.Value.HasFlag(LocalOrOnline.Online_Replays);
				}
				else
				{
					return UseRandomCarsFor.Value.HasFlag(LocalOrOnline.Local_Replays);
				}
			}
			return false;
		}

		#endregion

		#region SteamRivals Helpers

		public bool IsSteamRival(long userID, bool excludeSelf = false) => IsSteamRival(unchecked((ulong)userID), excludeSelf);

		public bool IsSteamRival(ulong userID, bool excludeSelf = false)
		{
			if (SteamworksManager.GetSteamID() == userID)
			{
				return !excludeSelf && UseRivalStyleForSelf.Value; // SteamRivals ignores your own user ID in the list.
			}
			return SteamRivals.ContainsKey(userID);
		}

		public bool TryGetSteamRival(long userID, out string nameComment) => TryGetSteamRival(unchecked((ulong)userID), out nameComment);

		public bool TryGetSteamRival(ulong userID, out string nameComment)
		{
			return SteamRivals.TryGetValue(userID, out nameComment);
		}

		public bool AddSteamRival(long userID, string nameComment, bool autoSave = true) => AddSteamRival(unchecked((ulong)userID), nameComment, autoSave);

		public bool AddSteamRival(ulong userID, string nameComment, bool autoSave = true)
		{
			if (nameComment == null)
			{
				nameComment = string.Empty; // Default to empty string I guess? It doesn't really matter either way, but would be more user friendly.
			}

			var steamRivals = SteamRivals;
			if (!steamRivals.ContainsKey(userID))
			{
				steamRivals[userID] = nameComment; // Name comment to make identifying users in Config.json easier.
				if (autoSave)
				{
					SaveDictionary(SteamRivals);
				}
				return true;
			}
			return false;
		}

		public bool RemoveSteamRival(long userID, bool autoSave = true) => RemoveSteamRival(unchecked((ulong)userID), autoSave);

		public bool RemoveSteamRival(ulong userID, bool autoSave = true)
		{
			if (SteamRivals.Remove(userID))
			{
				if (autoSave)
				{
					SaveDictionary(SteamRivals);
				}
				return true;
			}
			return false;
		}

		public int CountSteamRivals(IEnumerable<long> userIDs, bool countSelf = true)
		{
			return CountSteamRivals(userIDs.Select((userID) => unchecked((ulong)userID)), countSelf);
		}

		public int CountSteamRivals(IEnumerable<ulong> userIDs, bool countSelf = true)
		{
			//var useRivalStyleForSelf = this.UseRivalStyleForSelf;
			ulong selfID = SteamworksManager.GetSteamID();
			var steamRivals = SteamRivals;

			return userIDs.Count((userID) => (userID == selfID ? countSelf : steamRivals.ContainsKey(userID)));
		}

		#endregion

		#region Randomized Cars Helpers

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
					seed = unchecked(Environment.TickCount + unfixedRandomCount++);
					break;
			}
			System.Random rng = new System.Random(seed ^ unchecked((int)ExtraRandomnessSeed.Value));

			for (int i = 0; i < skipCount; i++)
			{
				rng.NextDouble();
			}
			return rng;
		}

		public CarReplayData.CarData ChooseRandomCarData(CarReplayData.CarData origCarData, int replaySeed, int placementSeed,
														 List<RandomCarType> carTypes, List<RandomColorPreset> colorPresets)
		{
			var carRng = GetRandom(replaySeed, placementSeed, RandomCarSeedMethod.Value, 0);
			var colorRng = GetRandom(replaySeed, placementSeed, RandomColorSeedMethod.Value, 1);

			// ==== Choose our car type ====

			if (!RandomCarType.TryCreate(origCarData.name_, 1f, 1, false, out var carType))
			{
				carType = RandomCarType.DefaultCarType;
			}

			var carMethod = RandomCarChoiceMethod.Value;
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
			var colorMethod = RandomColorChoiceMethod.Value;
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

			if (RandomColorSeedMethod.Value == RandomSeedMethod.By_Placement)
			{
				// Skip default color preset when coloring by placement (since the color varies by car).
				colorPresets.RemoveAll((x) => x.IsDefault);
			}

			return colorPresets;
		}

		//TEST THIS NEXT TIME
		public List<RandomCarType> LoadRandomCarTypes()
		{
			Dictionary<string, RandomCarType> randomCarTypes = new Dictionary<string, RandomCarType>();
			int explicitCustomCount = 0;
			bool requireUnlocks = RandomRequireCarUnlocks.Value;

			foreach (var carWeightPair in RandomCarWeights)
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

			float implicitCustomWeight = RandomCustomCarsDefaultWeight.Value;
			if (implicitCustomCount > 0 && implicitCustomWeight > 0f)
			{
				if (RandomCustomCarsSplitDefaultWeight.Value)
				{
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

		public Dictionary<string, float> GetDefaultCarWeights()
		{
			return new Dictionary<string, float>
			{
				{ "Spectrum",    1.00f * MaxRandomWeight },
				{ "Archive",     0.75f * MaxRandomWeight }, // (Adventure complete car)
				{ "Interceptor", 0.50f * MaxRandomWeight }, // (Nitronic Rush car)
				{ "Encryptor",   0.08f * MaxRandomWeight }, // (Doot car)
				{ "Halcyon",     0.05f * MaxRandomWeight }, // (Shiny Spectrum car)
				//{ "Catalyst",    0.00f }, // (Kickstarter Backer car, disabled for those that don't have this car unlocked)
			};
		}

		#endregion
	}
}
