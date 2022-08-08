using Centrifuge.Distance.Game;
using Centrifuge.Distance.GUI.Data;
using Centrifuge.Distance.GUI.Menu;
using Distance.ReplayIntensifies.Data;
using Distance.ReplayIntensifies.Helpers;
using Distance.ReplayIntensifies.Randomizer;
using Distance.ReplayIntensifies.Scripts;
using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using Reactor.API.Logging;
using Reactor.API.Runtime.Patching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Distance.ReplayIntensifies
{
	/// <summary>
	/// The mod's main class containing its entry point
	/// </summary>
	[ModEntryPoint("com.github.trigger-segfault/Distance.ReplayIntensifies")]
	public sealed class Mod : MonoBehaviour
	{
		public const string Name = "ReplayIntensifies";
		public const string FullName = "Distance." + Name;
		public const string FriendlyName = "Replay Intensifies";

		public const int OriginalMaxReplays = 20;
		public const int OriginalMaxSavedLocalReplays = 20;
		public const int OriginalMaxOnlineLeaderboards = 1000;

		public const int MaxReplaysAtAll = 1000;
		public const int MaxSavedLocalReplaysAtAll = 10000;
		public const int MaxOnlineLeaderboardsAtAll = 10000;

		public const int MinOnlineLeaderboards = 15;

		public const float MaxRandomWeight = 100f;

		// Keyword used in input prompts to randomize the result.
		public const string RngKeyword = "*";

		public const bool Debug_TestBackerCars = false;

		// In-Focus LODs are only used for the replay car with camera focus.
		// Unfocused cars should never have an LOD higher than Near. And having a lower max LOD is only
		//  necessary for unfocused cars, so remove these options and exclude both In-Focus's from Max LOD checks.
		public const CarLevelOfDetail.Level MaxMaxLevelOfDetail = CarLevelOfDetail.Level.Near; // Below In-Focus.
		public const CarLevelOfDetail.Level MinMaxLevelOfDetail = CarLevelOfDetail.Level.Speck; // or VeryFar
		public const CarLevelOfDetail.Level MaxMinLevelOfDetail = CarLevelOfDetail.Level.InFocus; // Allow forcing up to In-Focus



		public static Mod Instance { get; private set; }

		public IManager Manager { get; private set; }

		public Log Logger { get; private set; }

		public ConfigurationLogic Config { get; private set; }

		// Needed to update slider values after a set-all action,
		// and also prevent these same sliders from forcing the value back
		private CentrifugeMenu currentCentrifugeMenu;

		// Keep hold of these to populate at a later time.
		private MenuTree randomVanillaCarWeightsSubmenu;
		private MenuTree randomCustomCarWeightsSubmenu;

		/// <summary>
		/// Method called as soon as the mod is loaded.
		/// WARNING:	Do not load asset bundles/textures in this function
		///				The unity assets systems are not yet loaded when this
		///				function is called. Loading assets here can lead to
		///				unpredictable behaviour and crashes!
		/// </summary>
		public void Initialize(IManager manager)
		{
			// Do not destroy the current game object when loading a new scene
			DontDestroyOnLoad(this);

			Instance = this;
			Manager = manager;

			Logger = LogManager.GetForCurrentAssembly();
			Logger.Info(Mod.Name + ": Initializing...");

			Config = this.gameObject.AddComponent<ConfigurationLogic>();

			try
			{
				// Never ever EVER use this!!!
				// It's the same as below (with `GetCallingAssembly`) wrapped around a silent catch-all.
				//RuntimePatcher.AutoPatch();

				RuntimePatcher.HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
			}
			catch (Exception ex)
			{
				Logger.Error(Mod.Name + ": Error during Harmony.PatchAll()");
				Logger.Exception(ex);
				throw;
			}

			try
			{
				SteamworksHelper.Init(); // Handle this here for early error reporting.
			}
			catch (Exception ex)
			{
				Logger.Error(Mod.Name + ": Error during SteamworksHelper.Init()");
				Logger.Exception(ex);
				throw;
			}

			try
			{
				CreateSettingsMenu();
				// Subscribe to menu open so that we have a handle to the current CentrifugeMenu.
				// We need it to ensure controls are reset after other settings change specific values.
				// This isn't just to reflect changes in the controls either, some controls will outright FORCE their value
				//  setter after coming back from a message box, so the only way to stop that is to reset the menu controls.
				Events.GUI.MenuOpened.Subscribe(OnCentrifugeMenuOpened);
			}
			catch (Exception ex)
			{
				Logger.Error(Mod.Name + ": Error during CreateSettingsMenu()");
				Logger.Exception(ex);
				throw;
			}

			Logger.Info(Mod.Name + ": Initialized!");
		}

		private void OnCentrifugeMenuOpened(Events.GUI.MenuOpened.Data data)
		{
			this.currentCentrifugeMenu = data.menu;

			// Ensure these menus are populated after all car data is loaded.
			//  (this function will only run the first time it's called).
			// NOTE: This event is called *after* a menu is opened and its controls are displayed,
			//       so we're relying on hitting this event in a parent menu first.
			PopulateCarWeightsSubmenus();
		}

		#region Settings Helpers

		private void ResetCentrifugeMenuControls()
		{
			// Update all slider values on the page by rebuilding the page controls.
			if (this.currentCentrifugeMenu != null)
			{
				// FUN TIMES: After leaving the message box, all sliders have their Start function called.
				//            Guess what happens in Start? onChange is triggered.
				//            Guess what happens in onChange? setFn using the slider value is called.
				// SO TIME TO TEAR ALL THIS CRAP OUT AND STOP IT FROM DOING ANY
				// DAMAGE BY RESETTING THE MENU BECAUSE OH GOD THIS IS AWFUL.

				// Everything is rebuilt here, so it doesn't matter how badly we mess things up.
				this.currentCentrifugeMenu.SwitchPage(0, true, true);
			}
		}

		/*private static Dictionary<string, CarStyle> GetCarStyleSettingsEntries()
		{
			return new Dictionary<string, CarStyle>
			{
				{ "Ghost (Outline)",        CarStyle.Ghost_Outline },
				{ "Ghost (no Outline)",     CarStyle.Ghost_NoOutline },
				{ "Networked (Outline)",    CarStyle.Networked_Outline },
				{ "Networked (no Outline)", CarStyle.Networked_NoOutline },
				{ "Replay (Outline)",       CarStyle.Replay_Outline },
				{ "Replay (no Outline)",    CarStyle.Replay_NoOutline },
			};
		}*/

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
			if (string.IsNullOrEmpty(input) || input == RngKeyword)
			{
				return null;
			}
			else if (!float.TryParse(input, out float result) || float.IsNaN(result) || float.IsInfinity(result))
			{
				return "This is not a valid decimal number or " + RngKeyword;
			}
			return null;
		}

		private void OnSubmitFinishPreSpectateTime(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				// Don't change
			}
			else if (float.TryParse(input, out float result) && !float.IsNaN(result) && !float.IsInfinity(result))
			{
				// Allow extremely high times for essentially disabling finish-spectating.
				Config.FinishPreSpectateTime = Mathf.Clamp(result, 0.02f, 1_000_000f);
			}
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
				if (Config.RivalBrightness != result)
				{
					Config.RivalBrightness = result;

					// Treat changes to this value as an event since we don't have many other options for detecting if our own menu is closed.
					//  (GSL never broadcasts the Events.GUI.MenuClosed static event)
					// It's important to broadcast this so that outline brightnesses can be updated mid-game.
					Events.ReplayOptionsMenu.MenuClose.Broadcast(null);
				}
			}
		}

		private string OnValidateExtraRandomnessSeed(string input)
		{
			if (string.IsNullOrEmpty(input) || input == RngKeyword)
			{
				return null;
			}
			else if (!uint.TryParse(input, out _))
			{
				return $"Not a valid integer between 0 and {uint.MaxValue} or " + RngKeyword;
			}
			return null;
		}

		private void OnSubmitExtraRandomnessSeed(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				// Don't change
			}
			else if (input == RngKeyword) // Generate a random number.
			{
				var rng = new System.Random();
				// System.Random.Next() only generates something like a 30-bit integer,
				//  so cut that in half and use two random numbers for the LOWORD and HIWORD.
				Config.ExtraRandomnessSeed = unchecked(((uint)rng.Next() & 0xffffu) | (((uint)rng.Next() & 0xffffu) << 16));
			}
			else if (uint.TryParse(input, NumberStyles.AllowHexSpecifier, null, out uint result))
			{
				Config.ExtraRandomnessSeed = result;
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
				Config.SetCarTypeChance(carName, (float)rng.NextDouble(), false);
			}
			Config.Save();
			ResetCentrifugeMenuControls();
		}

		private void SetCarWeights(IEnumerable<string> carNames, string filter, float weight)
		{
			weight = Mathf.Clamp(weight, 0.0f, MaxRandomWeight);
			foreach (string carName in carNames.Where(c => FilterCarType(c, filter)))
			{
				Config.SetCarTypeChance(carName, weight, false);
			}
			Config.Save();
			ResetCentrifugeMenuControls();
		}

		private void OnSubmitSetVanillaCarWeights(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				// Don't change
			}
			else if (input == RngKeyword)
			{
				SetRandomCarWeights(RandomCarType.AllowedVanillaCarNames, null);

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
			else if (input == RngKeyword)
			{
				SetRandomCarWeights(RandomCarType.CustomCarNames, filter);

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
			var defaultValues = Config.GetDefaultCarWeights();
			foreach (string carName in RandomCarType.AllowedVanillaCarNames)
			{
				defaultValues.TryGetValue(carName, out float defaultValue); // Default weight or 0f.

				Config.SetCarTypeChance(carName, defaultValue, false); // Hold off on auto-saving until afterwards.
			}
			Config.Save();
			ResetCentrifugeMenuControls();
		}

		private void OnAskResetVanillaCarWeights()
		{
			MessageBox.Create("Are you sure you want to reset all vanilla car weights?", "RESET WEIGHTS")
				.SetButtons(Centrifuge.Distance.Data.MessageButtons.YesNo)
				.OnConfirm(OnConfirmResetVanillaCarWeights)
				.Show();
		}

		#endregion

		#region Settings Menu Create

		private void CreateSettingsMenu()
		{
			MenuTree settingsMenu = new MenuTree($"menu.mod.{Mod.Name.ToLower()}", Mod.FriendlyName);

			// Page 1
			settingsMenu.SubmenuButton(MenuDisplayMode.Both,
				"submenu:limits",
				"LIMITS SETTINGS",
				CreateLimitsSubmenu(),
				"Limits options for maximum number of live replays, saved local replays, and displayed online leaderboard ranks.");

			/*settingsMenu.SubmenuButton(MenuDisplayMode.Both,
				"submenu:quality",
				"QUALITY SETTINGS",
				CreateQualitySubmenu(),
				"Replay car rendering and performance options.");*/

			settingsMenu.SubmenuButton(MenuDisplayMode.Both,
				"submenu:replaymode",
				"REPLAY MODE SETTINGS",
				CreateReplayModeSubmenu(),
				"Replay Mode controls and behavior options.");

			// We can't check for Steam builds this early in initialization, so always show the menu.
			// It's fine, since the setting will always claim itself to be 'off' when `SteamworksManager.IsSteamBuild_` is false.
			settingsMenu.SubmenuButton(MenuDisplayMode.Both,
				"submenu:rivals",
				"STEAM RIVALS SETTINGS",
				CreateSteamRivalsSubmenu(),
				"Steam Rivals are users who're given their own ghost car style, so that you can spot your [i]true[/i] opponent from far away." +
				" Users can be changed from the level select leaderboards menu, or by editing Settings/Config.json.");

			settingsMenu.SubmenuButton(MenuDisplayMode.Both,
				"submenu:randomized_cars",
				"RANDOMIZED CARS SETTINGS",
				CreateRandomizedCarsSubmenu(),
				"Randomized cars allows changing up the cars and colors used by opponents. This can be useful when racing against local replays that all use the same car.");

			settingsMenu.ListBox<GhostOrReplay>(MenuDisplayMode.Both,
				"setting:use_data_effect_for_mode",
				"USE DATA EFFECT FOR MODE",
				() => Config.UseDataEffectForMode,
				(value) => Config.UseDataEffectForMode = value,
				GhostOrReplayExtensions.GetSettingsEntries(),
				"Which modes to use the data materialization spawn/finish-despawn effect in for non-ghost car styles.");

			// Settings in the root menu that are accessed the most often (from my own experience).
			settingsMenu.ListBox<CarStyle>(MenuDisplayMode.Both,
				"setting:ghost_car_style",
				"GHOST CAR STYLE",
				() => CarStyleExtensions.ToCarStyle(Config.GhostDetailType, Config.GhostOutline),
				(value) => {
					Config.GhostDetailType = value.GetDetailType();
					Config.GhostOutline = value.HasOutline();
				},
				CarStyleExtensions.GetSettingsEntries(),
				"Change the visual detail type of Ghost cars.");

			settingsMenu.ListBox<CarStyle>(MenuDisplayMode.Both,
				"setting:replay_car_style",
				"REPLAY CAR STYLE",
				() => CarStyleExtensions.ToCarStyle(Config.ReplayDetailType, Config.ReplayOutline),
				(value) => {
					Config.ReplayDetailType = value.GetDetailType();
					Config.ReplayOutline = value.HasOutline();
				},
				CarStyleExtensions.GetSettingsEntries(),
				"Change the visual detail type of Replay Mode cars.");


			settingsMenu.ListBox<CarLevelOfDetail.Level>(MenuDisplayMode.Both,
				"setting:max_level_of_detail",
				"MAX CAR LEVEL OF DETAIL",
				() => Config.MaxLevelOfDetail,
				(value) => Config.MaxLevelOfDetail = value,
				GetMaxLevelOfDetailSettingsEntries(),
				"Change the highest level of detail that opponent cars will render with." +
				" Lowering Max Level of Detail can improve performance when playing with more ghosts.");

			// NOTE: Min LOD has to be removed due to affecting the level environment.
			settingsMenu.ListBox<CarLevelOfDetail.Level>(MenuDisplayMode.Both,
				"setting:min_level_of_detail",
				"MIN CAR LEVEL OF DETAIL",
				() => Config.MinLevelOfDetail,
				(value) => Config.MinLevelOfDetail = value,
				GetMinLevelOfDetailSettingsEntries(),
				"Change the lowest level of detail that opponent cars will render with." +
				" Raising Min Level of Detail can decrease performance when playing with more ghosts." +
				" NOTE: In-Focus will force a car's LOD to be higher than normal for non-camera-focused cars.");

			// Page 2

			Menus.AddNew(MenuDisplayMode.Both, settingsMenu,
				Mod.FriendlyName.ToUpper(),
				"Settings for replay limits, leaderboard limits, and car rendering.");
		}

		private MenuTree CreateLimitsSubmenu()
		{
			MenuTree limitsSubmenu = new MenuTree($"submenu.mod.{Mod.Name.ToLower()}.limits", "Replay & Leaderboard Limits");

			// Page 1
			limitsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:use_max_selected_replays",
				"USE MAX SELECTED REPLAYS",
				() => Config.EnableSeparateMaxForSelectedReplays,
				(value) => Config.EnableSeparateMaxForSelectedReplays = value,
				"Use a separate maximum for the number of selected ghosts from the leaderboards menu.");

			limitsSubmenu.IntegerSlider(MenuDisplayMode.Both,
				"setting:max_auto_replays",
				"MAX AUTO REPLAYS",
				() => G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_,
				(value) => {
					G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_ = value;
					// Make sure to save changes, since ReplaySettings only saves after its own menu is closed.
					G.Sys.OptionsManager_.Replay_.Save();
				},
				1, Mod.MaxReplaysAtAll,
				5,
				"Maximum number of ghosts that will auto-load when playing a level. This is the [i]GHOSTS IN ARCADE COUNT[/i] option from the Replays menu, and is included here for convenience.");

			limitsSubmenu.IntegerSlider(MenuDisplayMode.Both,
				"setting:max_selected_replays",
				"MAX SELECTED REPLAYS",
				() => Config.MaxSelectedReplays,
				(value) => Config.MaxSelectedReplays = value,
				Mod.OriginalMaxReplays, Mod.MaxReplaysAtAll,
				Mod.OriginalMaxReplays,
				"Maximum number of ghosts that will be loaded when selecting from the leaderboards menu.");


			limitsSubmenu.IntegerSlider(MenuDisplayMode.Both,
				"setting:max_online_leaderboards",
				"MAX ONLINE LEADERBOARD RANKS",
				() => Config.MaxOnlineLeaderboards,
				(value) => Config.MaxOnlineLeaderboards = value,
				Mod.MinOnlineLeaderboards, Mod.MaxOnlineLeaderboardsAtAll,
				Mod.OriginalMaxOnlineLeaderboards,
				"Maximum number of leaderboard ranks shown for Friends and Online tabs.");

			limitsSubmenu.IntegerSlider(MenuDisplayMode.Both,
				"setting:max_saved_local_replays",
				"MAX SAVED LOCAL REPLAYS",
				() => Config.MaxSavedLocalReplays,
				(value) => Config.MaxSavedLocalReplays = value,
				Mod.OriginalMaxSavedLocalReplays, Mod.MaxSavedLocalReplaysAtAll,
				500,
				"Maximum number of local leaderboard replays that will be saved.\n[FF0000]WARNING:[-] Completing a map with more than this number of ghosts will remove ALL ghosts past the maximum.");

			limitsSubmenu.ListBox<LocalLeaderboardTrimming>(MenuDisplayMode.Both,
				"setting:local_replay_trimming",
				"LOCAL REPLAY TRIMMING",
				() => Config.LocalReplayTrimming,
				(value) => Config.LocalReplayTrimming = value,
				LocalLeaderboardTrimmingExtensions.GetSettingsEntries(),
				"When creating a local leaderboard replay past the placement limit, choose whether replays are Never deleted, only your Current Run gets deleted, or if all replays past the limit Always get deleted.");

			limitsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:fill_with_local_replays",
				"FILL WITH LOCAL REPLAYS",
				() => Config.FillWithLocalReplays,
				(value) => Config.FillWithLocalReplays = value,
				"Fill remaining auto slots with local replays when there aren't enough online replays to load.");

			limitsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:enable_unrestricted_colors",
				"ENABLE UNRESTRICTED OPPONENT COLORS",
				() => Config.EnableUnrestrictedOpponentColors,
				(value) => Config.EnableUnrestrictedOpponentColors = value,
				"Online opponents and non-[i]Ghost Detail Type[/i] cars will NOT have their colors clamped, allowing for extremely bright cars." +
				" Bright cars are made by editing color preset files and changing the color channels to very large values.");

			return limitsSubmenu;
		}

		private MenuTree CreateReplayModeSubmenu()
		{
			MenuTree replayModeSubmenu = new MenuTree($"submenu.mod.{Mod.Name.ToLower()}.replaymode", "Replay Mode");

			// Page 1
			replayModeSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:replaymode_disable_cinematic_cameras",
				"DISABLE CINEMATIC CAMERAS",
				() => Config.ReplayModeDisableCinematicCameras,
				(value) => Config.ReplayModeDisableCinematicCameras = value,
				"Turn off cinematic camera triggers in replay mode, allowing for more flexible camera movement.");

			replayModeSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:replaymode_pause_at_start",
				"PAUSE AT START",
				() => Config.ReplayModePauseAtStart,
				(value) => Config.ReplayModePauseAtStart = value,
				"Pause at match start during replay mode, allowing easier tracking of extremely short runs.");

			/*replayModeSubmenu.FloatSlider(MenuDisplayMode.Both,
				"setting:replaymode_pre_spectate_time",
				"FINISH PRE-SPECTATE TIME",
				() => Config.FinishPreSpectateTime,
				(value) => Config.FinishPreSpectateTime = value,
				0.02f, 300f,
				5.0f,
				"Change the number of seconds to wait after finishing a level before going into spectate mode.");*/

			// Use an InputPrompt to reduce the strain on constantly changing the setting and causing all rival cars to update their outline.
			replayModeSubmenu.InputPrompt(MenuDisplayMode.Both,
				"setting:replaymode_pre_spectate_time_input",
				"FINISH PRE-SPECTATE TIME",
				OnSubmitFinishPreSpectateTime,
				null,
				OnValidateFloat,
				"ENTER SECONDS",
				null,
				"Change the number of seconds to wait after finishing a level before going into spectate mode (default is 5 seconds).")
				.WithDefaultValue(() => Config.FinishPreSpectateTime.ToString()); // Need to use this since there's no method for function defaults.

			return replayModeSubmenu;
		}

		private MenuTree CreateSteamRivalsSubmenu()
		{
			MenuTree rivalsSubmenu = new MenuTree($"submenu.mod.{Mod.Name.ToLower()}.rivals", "Steam Rivals");

			// Page 1
			rivalsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:rivals_enabled",
				"ENABLE STEAM RIVALS",
				() => Config.EnableSteamRivals,
				(value) => Config.EnableSteamRivals = value,
				"Enable the Steam Rivals feature.");

			rivalsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:rivals_highlight_in_leaderboards",
				"HIGHLIGHT RIVALS IN LEADERBOARDS",
				() => Config.HighlightRivalsInLeaderboards,
				(value) => Config.HighlightRivalsInLeaderboards = value,
				"Steam Rivals listed in the level select leaderboards menu will be colored differently.");

			rivalsSubmenu.ListBox<GhostOrReplay>(MenuDisplayMode.Both,
				"setting:rival_use_style_for_mode",
				"USE CAR STYLE FOR MODE",
				() => Config.UseRivalStyleForMode,
				(value) => Config.UseRivalStyleForMode = value,
				GhostOrReplayExtensions.GetSettingsEntries(),
				"Which modes to use the Steam Rival car style in.");

			rivalsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:rival_use_style_for_self",
				"USE CAR STYLE FOR SELF",
				() => Config.UseRivalStyleForSelf,
				(value) => Config.UseRivalStyleForSelf = value,
				"Steam Rival car styles will also be used for your own ghosts.");

			rivalsSubmenu.ListBox<CarStyle>(MenuDisplayMode.Both,
				"setting:rival_car_style",
				"RIVAL CAR STYLE",
				() => CarStyleExtensions.ToCarStyle(Config.RivalDetailType, Config.RivalOutline),
				(value) => {
					Config.RivalDetailType = value.GetDetailType();
					Config.RivalOutline = value.HasOutline();
				},
				CarStyleExtensions.GetSettingsEntries(),
				"Change the visual detail type of Steam Rival cars.");

			/*rivalsSubmenu.FloatSlider(MenuDisplayMode.Both,
				"setting:rival_outline_brightness",
				"RIVAL OUTLINE BRIGHTNESS",
				() => Config.RivalBrightness,
				(value) => {

					Config.RivalBrightness = value;
					// Treat changes to this value as an event since we don't have many other options for detecting if our own menu is closed.
					//  (GSL never broadcasts the Events.GUI.MenuClosed static event)
					Events.ReplayOptionsMenu.MenuClose.Broadcast(null);
				},
				0.05f, 100_000f,
				1f,
				"Change the brightness for Steam Rival car outlines.");*/

			// Use an InputPrompt to reduce the strain on constantly changing the setting and causing all rival cars to update their outline.
			rivalsSubmenu.InputPrompt(MenuDisplayMode.Both,
				"setting:rival_outline_brightness_input",
				"RIVAL OUTLINE BRIGHTNESS",
				OnSubmitRivalOutlineBrightness,
				null,
				OnValidateFloat,
				"ENTER OUTLINE BRIGHTNESS",
				null,
				"Change the brightness for Steam Rival car outlines." +
				" NOTE: Brightness values higher than 1.0 will only increase the intensity of flames and wing trails.")
				.WithDefaultValue(() => Config.RivalBrightness.ToString()); // Need to use this since there's no method for function defaults.

			return rivalsSubmenu;
		}

		private MenuTree CreateRandomizedCarsSubmenu()
		{
			MenuTree randomSubmenu = new MenuTree($"submenu.mod.{Mod.Name.ToLower()}.random", "Randomized Cars");

			// Page 1
			randomSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:random_enabled",
				"ENABLE RANDOMIZED CARS",
				() => Config.EnableRandomizedCars,
				(value) => Config.EnableRandomizedCars = value,
				"Enable the randomized cars feature, allowing for more variety with opponents.");

			randomSubmenu.ListBox<LocalOrOnline>(MenuDisplayMode.Both,
				"setting:random_local_or_online_cars",
				"USE RANDOM CARS FOR",
				() => Config.UseRandomCarsFor,
				(value) => Config.UseRandomCarsFor = value,
				LocalOrOnlineExtensions.GetSettingsEntries(),
				"Choose whether local and/or online leaderboards replays will be randomized." +
				" 'Online' is not recommended for normal play, since a player's car can be considered part of 'their identity' in the leaderboards.");

			randomSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:random_rival_cars",
				"USE RANDOM RIVAL CARS",
				() => Config.UseRandomRivalCars,
				(value) => Config.UseRandomRivalCars = value,
				"Rival cars will be randomized (STEAM RIVALS feature must be enabled).");

			randomSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:random_respect_backer_cars",
				"RESPECT KICKSTARTER BACKERS",
				() => Config.RandomRespectBackerCars,
				(value) => Config.RandomRespectBackerCars = value,
				"Disable randomizing online replays that use the Kickstarter backer car.");

			randomSubmenu.InputPrompt(MenuDisplayMode.Both,
				"setting:extra_randomness",
				"EXTRA RANDOMNESS SEED",
				OnSubmitExtraRandomnessSeed,
				null,
				OnValidateExtraRandomnessSeed,
				"ENTER AN INTEGER",
				null,
				"Change up the fixed randomness for replays a little." +
				$" Enter a number between 0 and {uint.MaxValue} (0x{uint.MaxValue:X8}). Or enter " + RngKeyword + " to generate a random number.")
				.WithDefaultValue(() => Config.ExtraRandomnessSeed.ToString());


			const string SeedMethodDescription =
				" By Replay will use the replay data as the seed, so that each replay will always be the same." +
				" By Placement will use the placement between all replays as the seed.";

			const string ChoiceMethodDescription =
				" Cycle will randomly cycle through choices once before choosing duplicates.";

			randomSubmenu.ListBox<RandomSeedMethod>(MenuDisplayMode.Both,
				"setting:random_car_seed_method",
				"CAR SEED METHOD",
				() => Config.RandomCarSeedMethod,
				(value) => Config.RandomCarSeedMethod = value,
				RandomSeedMethodExtensions.GetSettingsEntries(),
				"Change how randomness is determined for car types." + SeedMethodDescription);

			randomSubmenu.ListBox<RandomSeedMethod>(MenuDisplayMode.Both,
				"setting:random_color_seed_method",
				"COLOR SEED METHOD",
				() => Config.RandomColorSeedMethod,
				(value) => Config.RandomColorSeedMethod = value,
				RandomSeedMethodExtensions.GetSettingsEntries(),
				"Change how randomness is determined for car colors." + SeedMethodDescription);

			randomSubmenu.ListBox<RandomCarMethod>(MenuDisplayMode.Both,
				"setting:random_car_choice_method",
				"CAR CHOICE METHOD",
				() => Config.RandomCarChoiceMethod,
				(value) => Config.RandomCarChoiceMethod = value,
				RandomCarMethodExtensions.GetSettingsEntries(),
				"Choose how random car types will be decided." + ChoiceMethodDescription);

			randomSubmenu.ListBox<RandomColorMethod>(MenuDisplayMode.Both,
				"setting:random_color_choice_method",
				"COLOR CHOICE METHOD",
				() => Config.RandomColorChoiceMethod,
				(value) => Config.RandomColorChoiceMethod = value,
				RandomColorMethodExtensions.GetSettingsEntries(),
				"Choose how random car colors will be decided." + ChoiceMethodDescription);

			// Page 2
			randomSubmenu.SubmenuButton(MenuDisplayMode.Both,
				"submenu:random_vanilla_car_weights",
				"VANILLA CAR WEIGHT SETTINGS",
				CreateVanillaCarWeightsSubmenu(),
				"Change weighted chances for vanilla cars to be randomly chosen.");

			randomSubmenu.SubmenuButton(MenuDisplayMode.Both,
				"submenu:random_custom_car_weights",
				"CUSTOM CAR WEIGHT SETTINGS",
				CreateCustomCarWeightsSubmenu(),
				"Change weighted chances for custom cars to be randomly chosen.");

			randomSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:random_require_car_unlocks",
				"REQUIRE CAR UNLOCKS",
				() => Config.RandomRequireCarUnlocks,
				(value) => Config.RandomRequireCarUnlocks = value,
				"Disable using random car types that you haven't unlocked yet.");

			return randomSubmenu;
		}

		private MenuTree CreateVanillaCarWeightsSubmenu()
		{
			MenuTree randomVanillaCarWeightsSubmenu = new MenuTree($"submenu.mod.{Mod.Name.ToLower()}.random.vanilla_car_chances", "Vanilla Car Weights");
			this.randomVanillaCarWeightsSubmenu = randomVanillaCarWeightsSubmenu;

			return randomVanillaCarWeightsSubmenu;
		}

		private MenuTree CreateCustomCarWeightsSubmenu()
		{
			MenuTree customSubmenu = new MenuTree($"submenu.mod.{Mod.Name.ToLower()}.random.custom_car_chances", "Custom Car Weights");
			this.randomCustomCarWeightsSubmenu = customSubmenu;

			return customSubmenu;
		}

		private const bool WeightsItalics = false;
		private const string WeightsPrefix  = (WeightsItalics) ? "[i]"  : "";
		private const string WeightsPostfix = (WeightsItalics) ? "[/i]" : "";

		private void PopulateCarWeightsSubmenus()
		{
			MenuTree vanillaSubmenu = this.randomVanillaCarWeightsSubmenu;
			MenuTree customSubmenu = this.randomCustomCarWeightsSubmenu;
			if (vanillaSubmenu.Count > 0 && customSubmenu.Count > 0)
			{
				return; // Already populated.
			}
			vanillaSubmenu.Clear();
			customSubmenu.Clear();


			vanillaSubmenu.InputPrompt(MenuDisplayMode.Both,
				"setting:set_vanilla_car_chances",
				WeightsPrefix + "SET ALL VANILLA CAR WEIGHTS" + WeightsPostfix,
				OnSubmitSetVanillaCarWeights,
				null,
				OnValidateFloatOrRng,
				"ENTER VANILLA WEIGHT",
				null,
				string.Format(CultureInfo.InvariantCulture, // Decimal characters differ by culture, so force invariant to match 0.0
				"Set the weighted chance for all matching custom cars. Enter a decimal number between 0.0 and {0:0.0}.", MaxRandomWeight) +
				" Or enter " + RngKeyword + " to assign random weights to each individual car.");

			vanillaSubmenu.ActionButton(MenuDisplayMode.Both,
				"setting:reset_vanilla_car_chances",
				WeightsPrefix + "RESET VANILLA CAR WEIGHTS" + WeightsPostfix,
				OnAskResetVanillaCarWeights,
				"All vanilla cars will be set back to their default weighted chances of appearing.");

			
			customSubmenu.InputPrompt(MenuDisplayMode.Both,
				"setting:set_custom_car_chances",
				WeightsPrefix + "SET ALL CUSTOM CAR WEIGHTS" + WeightsPostfix,
				OnSubmitSetCustomCarWeightsWithFilter,
				null,
				null,
				"ENTER FILTER/OR SKIP",
				null,
				"Enter a filter to match custom cars by name." +
				string.Format(CultureInfo.InvariantCulture, // Decimal characters differ by culture, so force invariant to match 0.0
				" Then set the weighted chance for all matching custom cars. Enter a decimal number between 0.0 and {0:0.0}.", MaxRandomWeight) +
				" Or enter " + RngKeyword + " to assign random weights to each individual car.");

			customSubmenu.FloatSlider(MenuDisplayMode.Both,
				"setting:random_custom_cars_default_chance",
				WeightsPrefix + "DEFAULT CUSTOM CARS WEIGHT" + WeightsPostfix,
				() => Config.RandomCustomCarsDefaultWeight,
				(value) => Config.RandomCustomCarsDefaultWeight = value,
				0.0f, MaxRandomWeight,
				0.0f,
				"Default weighted chance for any custom car with an individual weight of 0.0." +
				" Use this when you want all custom cars to have a chance of appearing, without having to touch the settings after new additions (0.0 to disable).");

			customSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:random_custom_cars_split_default_chance",
				WeightsPrefix + "SPLIT DEFAULT CUSTOM CARS WEIGHT" + WeightsPostfix,
				() => Config.RandomCustomCarsSplitDefaultWeight,
				(value) => Config.RandomCustomCarsSplitDefaultWeight = value,
				"Any custom car using the default weight will split the weighted chance of appearing among all custom cars, rather than being given the same weight (see option above).");


			var defaultCarChances = Config.GetDefaultCarWeights();
			foreach (var carType in RandomCarType.LoadAllCarTypes(false, 0f, 1))
			{
				string carName = carType.Name;
				
				defaultCarChances.TryGetValue(carName, out float defaultValue); // Default weight or 0f.

				var submenu = (carType.IsVanilla) ? vanillaSubmenu : customSubmenu;
				string customText = (carType.IsVanilla) ? "" : " custom";

				submenu.FloatSlider(MenuDisplayMode.Both,
					"setting:random_car_chance:" + carName,
					carName,//.ToUpper(),
					() => Config.GetCarTypeChance(carName),
					(value) => Config.SetCarTypeChance(carName, value),
					0.0f, MaxRandomWeight,
					defaultValue,
					$"Weighted chance that the '{carName}'{customText} car will be randomly chosen (0.0 to ignore this car).");
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
			if (Mod.Instance.Config.EnableSeparateMaxForSelectedReplays)
			{
				return Mathf.Clamp(Mod.Instance.Config.MaxSelectedReplays, Mod.OriginalMaxReplays, Mod.MaxReplaysAtAll);
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
			return Mathf.Clamp(Mod.Instance.Config.MaxSavedLocalReplays, Mod.OriginalMaxSavedLocalReplays, Mod.MaxSavedLocalReplaysAtAll);
		}

		/// <summary>
		/// Gets the max number of entries that will show up in Friends/Global leaderboards.
		/// </summary>
		/// <remarks>
		/// This exists to inline a passed argument operand.
		/// </remarks>
		public static int GetMaxOnlineLeaderboards()
		{
			return Mathf.Clamp(Mod.Instance.Config.MaxOnlineLeaderboards, Mod.MinOnlineLeaderboards, Mod.MaxOnlineLeaderboardsAtAll);
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
				return !Mod.Instance.Config.EnableUnrestrictedOpponentColors;
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
			if (!Mod.Instance.Config.EnableSteamRivals || !Mod.Instance.Config.HighlightRivalsInLeaderboards)
			{
				return;
			}

			LevelSelectLeaderboardMenu.Entry entry = button.entry_ as LevelSelectLeaderboardMenu.Entry;

			Color color = (!entry.info_.isLocal_) ? Color.white : LevelSelectLeaderboardButton.localColor_;
			bool isRival = false;

			if (!entry.info_.isLocal_ && entry.leaderboardEntry_ is SteamworksLeaderboard.Entry steamEntry)
			{
				ulong steamID = SteamworksHelper.GetLeaderboardEntrySteamID(steamEntry);
				if (Mod.Instance.Config.IsSteamRival(steamID, true))
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
	}
}
