using Centrifuge.Distance.Game;
using Centrifuge.Distance.GUI.Data;
using Distance.ReplayIntensifies.Helpers;
using Distance.ReplayIntensifies.Scripts;
using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using Reactor.API.Logging;
using Reactor.API.Runtime.Patching;
using System;
using System.Collections.Generic;
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
			}
			catch (Exception ex)
			{
				Logger.Error(Mod.Name + ": Error during CreateSettingsMenu()");
				Logger.Exception(ex);
				throw;
			}

			Logger.Info(Mod.Name + ": Initialized!");
		}

		#region Settings Helpers

		private enum CarStyle
		{
			Ghost_Outline,
			Ghost_NoOutline,
			Networked_Outline,
			Networked_NoOutline,
			Replay_Outline,
			Replay_NoOutline,
		}

		private static CarStyle ToCarStyle(CarLevelOfDetail.Type detailType, bool outline)
		{
			switch (detailType)
			{
			case CarLevelOfDetail.Type.Ghost:
				return (outline) ? CarStyle.Ghost_Outline : CarStyle.Ghost_NoOutline;

			case CarLevelOfDetail.Type.Networked:
				return (outline) ? CarStyle.Networked_Outline : CarStyle.Networked_NoOutline;

			case CarLevelOfDetail.Type.Replay:
				return (outline) ? CarStyle.Replay_Outline : CarStyle.Replay_NoOutline;

			default:
				goto case CarLevelOfDetail.Type.Ghost; // Fallback for invalid type
			}
		}

		private static bool OutlineFromCarStyle(CarStyle carStyle)
		{
			switch (carStyle)
			{
			case CarStyle.Ghost_Outline:
			case CarStyle.Networked_Outline:
			case CarStyle.Replay_Outline:
				return true;

			case CarStyle.Ghost_NoOutline:
			case CarStyle.Networked_NoOutline:
			case CarStyle.Replay_NoOutline:
				return false;

			default:
				return false; // Fallback for invalid type
			}
		}

		private static CarLevelOfDetail.Type DetailTypeFromCarStyle(CarStyle carStyle)
		{
			switch (carStyle)
			{
			case CarStyle.Ghost_Outline:
			case CarStyle.Ghost_NoOutline:
				return CarLevelOfDetail.Type.Ghost;

			case CarStyle.Networked_Outline:
			case CarStyle.Networked_NoOutline:
				return CarLevelOfDetail.Type.Networked;

			case CarStyle.Replay_Outline:
			case CarStyle.Replay_NoOutline:
				return CarLevelOfDetail.Type.Replay;

			default:
				return CarLevelOfDetail.Type.Ghost; // Fallback for invalid type
			}
		}

		#endregion

		private void CreateSettingsMenu()
		{
			Dictionary<string, CarStyle> carStyleEntries = new Dictionary<string, CarStyle>
			{
				{ "Ghost (Outline)",        CarStyle.Ghost_Outline },
				{ "Ghost (no Outline)",     CarStyle.Ghost_NoOutline },
				{ "Networked (Outline)",    CarStyle.Networked_Outline },
				{ "Networked (no Outline)", CarStyle.Networked_NoOutline },
				{ "Replay (Outline)",       CarStyle.Replay_Outline },
				{ "Replay (no Outline)",    CarStyle.Replay_NoOutline },
			};

			/*Dictionary<string, CarLevelOfDetail.Type> detailTypeEntries = new Dictionary<string, CarLevelOfDetail.Type>
			{
				{ "Ghost",     CarLevelOfDetail.Type.Ghost },     // 2
				{ "Networked", CarLevelOfDetail.Type.Networked }, // 0
				{ "Replay",    CarLevelOfDetail.Type.Replay },    // 1
			};*/

			/*Dictionary<string, CarLevelOfDetail.Level> detailLevelEntries = new Dictionary<string, CarLevelOfDetail.Level>
			{
				{ "In-Focus (First Person)", CarLevelOfDetail.Level.InFocusFP }, // 0
				{ "In-Focus",                CarLevelOfDetail.Level.InFocus },   // 1
				{ "Near",                    CarLevelOfDetail.Level.Near },      // 2
				{ "Medium",                  CarLevelOfDetail.Level.Medium },    // 3
				{ "Far",                     CarLevelOfDetail.Level.Far },       // 4
				{ "Very Far",                CarLevelOfDetail.Level.VeryFar },   // 5
				{ "Speck",                   CarLevelOfDetail.Level.Speck },     // 6
			};*/

			Dictionary<string, CarLevelOfDetail.Level> detailLevelEntries = new Dictionary<string, CarLevelOfDetail.Level>
			{
				{ "Very Low",                CarLevelOfDetail.Level.Speck },     // 6
				{ "Low",                     CarLevelOfDetail.Level.VeryFar },   // 5
				{ "Medium",                  CarLevelOfDetail.Level.Far },       // 4
				{ "High",                    CarLevelOfDetail.Level.Medium },    // 3
				{ "Ultra",                   CarLevelOfDetail.Level.Near },      // 2
				{ "Ultra (In-Focus)",        CarLevelOfDetail.Level.InFocus },   // 1
				{ "Ultra (First Person)",    CarLevelOfDetail.Level.InFocusFP }, // 0
			};

			var maxDetailLevelEntries = new Dictionary<string, CarLevelOfDetail.Level>(detailLevelEntries);
			var minDetailLevelEntries = new Dictionary<string, CarLevelOfDetail.Level>(detailLevelEntries);
			// Remove unsupported LODs from min/max entries.
			foreach (var lodPair in detailLevelEntries)
			{
				if (lodPair.Value < Mod.MaxMaxLevelOfDetail || lodPair.Value > Mod.MinMaxLevelOfDetail)
				{
					maxDetailLevelEntries.Remove(lodPair.Key);
				}
				if (lodPair.Value < Mod.MaxMinLevelOfDetail)
				{
					minDetailLevelEntries.Remove(lodPair.Key);
				}
			}



			MenuTree settingsMenu = new MenuTree("menu.mod." + Mod.Name.ToLower(), Mod.FriendlyName);

			// Page 1
			settingsMenu.CheckBox(MenuDisplayMode.Both,
				"setting:use_max_selected_replays",
				"USE MAX SELECTED REPLAYS",
				() => Config.EnableSeparateMaxForSelectedReplays,
				(value) => Config.EnableSeparateMaxForSelectedReplays = value,
				"Use a separate maximum for the number of selected ghosts from the leaderboards menu.");

			settingsMenu.IntegerSlider(MenuDisplayMode.Both,
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

			settingsMenu.IntegerSlider(MenuDisplayMode.Both,
				"setting:max_selected_replays",
				"MAX SELECTED REPLAYS",
				() => Config.MaxSelectedReplays,
				(value) => Config.MaxSelectedReplays = value,
				Mod.OriginalMaxReplays, Mod.MaxReplaysAtAll,
				Mod.OriginalMaxReplays,
				"Maximum number of ghosts that will be loaded when selecting from the leaderboards menu.");


			settingsMenu.IntegerSlider(MenuDisplayMode.Both,
				"setting:max_saved_local_replays",
				"MAX SAVED LOCAL REPLAYS",
				() => Config.MaxSavedLocalReplays,
				(value) => Config.MaxSavedLocalReplays = value,
				Mod.OriginalMaxSavedLocalReplays, Mod.MaxSavedLocalReplaysAtAll,
				500,
				"Maximum number of local leaderboard replays that will be saved.\n[FF0000]WARNING:[-] Completing a map with more than this number of ghosts will remove ALL ghosts past the maximum.");

			settingsMenu.IntegerSlider(MenuDisplayMode.Both,
				"setting:max_online_leaderboards",
				"MAX ONLINE LEADERBOARD RANKS",
				() => Config.MaxOnlineLeaderboards,
				(value) => Config.MaxOnlineLeaderboards = value,
				Mod.MinOnlineLeaderboards, Mod.MaxOnlineLeaderboardsAtAll,
				Mod.OriginalMaxOnlineLeaderboards,
				"Maximum number of leaderboard ranks shown for Friends and Online tabs.");


			settingsMenu.ListBox<CarStyle>(MenuDisplayMode.Both,
				"setting:ghost_car_style",
				"GHOST CAR STYLE",
				() => Mod.ToCarStyle(Config.GhostDetailType, Config.GhostOutline),
				(value) => {
					Config.GhostDetailType = Mod.DetailTypeFromCarStyle(value);
					Config.GhostOutline = Mod.OutlineFromCarStyle(value);
				},
				carStyleEntries,
				"Change the visual detail type of Ghost cars.");

			settingsMenu.ListBox<CarStyle>(MenuDisplayMode.Both,
				"setting:replay_car_style",
				"REPLAY CAR STYLE",
				() => Mod.ToCarStyle(Config.ReplayDetailType, Config.ReplayOutline),
				(value) => {
					Config.ReplayDetailType = Mod.DetailTypeFromCarStyle(value);
					Config.ReplayOutline = Mod.OutlineFromCarStyle(value);
				},
				carStyleEntries,
				"Change the visual detail type of Replay Mode cars.");


			settingsMenu.ListBox<CarLevelOfDetail.Level>(MenuDisplayMode.Both,
				"setting:max_level_of_detail",
				"MAX CAR LEVEL OF DETAIL",
				() => Config.MaxLevelOfDetail,
				(value) => Config.MaxLevelOfDetail = value,
				maxDetailLevelEntries,
				"Change the highest level of detail that opponent cars will render with." +
				" Lowering Max Level of Detail can improve performance when playing with more ghosts.");

			// NOTE: Min LOD has to be removed due to affecting the level environment.
			settingsMenu.ListBox<CarLevelOfDetail.Level>(MenuDisplayMode.Both,
				"setting:min_level_of_detail",
				"MIN CAR LEVEL OF DETAIL",
				() => Config.MinLevelOfDetail,
				(value) => Config.MinLevelOfDetail = value,
				minDetailLevelEntries,
				"Change the lowest level of detail that opponent cars will render with." +
				" Raising Min Level of Detail can decrease performance when playing with more ghosts." +
				" NOTE: In-Focus will force a car's LOD to be higher than normal for non-camera-focused cars.");


			// Page 2
			// Put this submenu at the top of the second page, for faster access over the less-important "ENABLE UNRESTRICTED OPPONENT COLORS".
			// We can't check for Steam builds this early in initialization, so always show the menu.
			// It's fine, since the setting will always claim itself to be 'off' when `SteamworksManager.IsSteamBuild_` is false.
			//if (SteamworksManager.IsSteamBuild_)
			//{
				settingsMenu.SubmenuButton(MenuDisplayMode.Both,
					"submenu:rivals",
					"STEAM RIVALS SETTINGS", // "\u25B6" Black right-pointing triangle
					CreateSteamRivalsSubmenu(carStyleEntries),
					"EXPERIMENTAL: Steam Rivals are users who're given their own ghost car style, so that you can spot your [i]true[/i] opponent from far away." +
					" Users can be changed from the level select leaderboards menu, or by editing Settings/Config.json.");
			//}

			settingsMenu.CheckBox(MenuDisplayMode.Both,
				"setting:enable_unrestricted_colors",
				"ENABLE UNRESTRICTED OPPONENT COLORS",
				() => Config.EnableUnrestrictedOpponentColors,
				(value) => Config.EnableUnrestrictedOpponentColors = value,
				"Online opponents and non-[i]Ghost Detail Type[/i] cars will NOT have their colors clamped, allowing for extremely bright cars." +
				" Bright cars are made by editing color preset files and changing the color channels to very large values.");

			settingsMenu.CheckBox(MenuDisplayMode.Both,
				"setting:data_effect_in_ghost_mode",
				"DATA EFFECT IN GHOST MODE",
				() => Config.ShowDataEffectInGhostMode,
				(value) => Config.ShowDataEffectInGhostMode = value,
				"The data materialization spawn effect will be used when racing non-ghost cars.");



			Menus.AddNew(MenuDisplayMode.Both, settingsMenu,
				Mod.FriendlyName.ToUpper(),
				"Settings for replay limits, leaderboard limits, and car rendering.");
		}

		private MenuTree CreateSteamRivalsSubmenu(Dictionary<string, CarStyle> carStyleEntries)
		{
			MenuTree rivalsSubmenu = new MenuTree("submenu.mod." + Mod.Name.ToLower() + ".rivals", "Steam Rivals");

			rivalsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:rivals_enabled",
				"ENABLE STEAM RIVALS",
				() => Config.EnableSteamRivals,
				(value) => Config.EnableSteamRivals = value,
				"Turns on this experimental feature.");

			rivalsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:rivals_highlight_in_leaderboards",
				"HIGHLIGHT RIVALS IN LEADERBOARDS",
				() => Config.HighlightRivalsInLeaderboards,
				(value) => Config.HighlightRivalsInLeaderboards = value,
				"Steam Rivals listed in the level select leaderboards menu will be colored differently.");

			rivalsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:rival_use_style_for_ghosts",
				"USE CAR STYLE FOR GHOST",
				() => Config.UseRivalStyleForGhosts,
				(value) => Config.UseRivalStyleForGhosts = value,
				"Steam Rival car styles will also be used when racing other ghosts.");

			rivalsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:rival_use_style_for_replays",
				"USE CAR STYLE FOR REPLAYS",
				() => Config.UseRivalStyleForReplays,
				(value) => Config.UseRivalStyleForReplays = value,
				"Steam Rival car styles will also be used when in Replay Mode.");

			rivalsSubmenu.CheckBox(MenuDisplayMode.Both,
				"setting:rival_use_style_for_self",
				"USE CAR STYLE FOR SELF",
				() => Config.UseRivalStyleForSelf,
				(value) => Config.UseRivalStyleForSelf = value,
				"Steam Rival car styles will also be used for your own ghosts.");

			rivalsSubmenu.ListBox<CarStyle>(MenuDisplayMode.Both,
				"setting:rival_car_style",
				"RIVAL CAR STYLE",
				() => Mod.ToCarStyle(Config.RivalDetailType, Config.RivalOutline),
				(value) => {
					Config.RivalDetailType = Mod.DetailTypeFromCarStyle(value);
					Config.RivalOutline = Mod.OutlineFromCarStyle(value);
				},
				carStyleEntries,
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
				(value) => {
					if (float.TryParse(value, out float result) && !float.IsNaN(result) && !float.IsInfinity(result))
					{
						// Allow extremely high outline brightness (which doesn't affect outline itself, but does affect jets and wings).
						// Anything higher than 100,000 will produce black splotches from the intensity.
						result = Math.Max(0.05f, Math.Min(100_000f, result));
						if (Config.RivalBrightness != result)
						{
							Config.RivalBrightness = result;

							// Treat changes to this value as an event since we don't have many other options for detecting if our own menu is closed.
							//  (GSL never broadcasts the Events.GUI.MenuClosed static event)
							// It's important to broadcast this so that outline brightnesses can be updated mid-game.
							Events.ReplayOptionsMenu.MenuClose.Broadcast(null);
							Logger.Debug($"RIVAl OUTLINE BRIGHTNESS: Value changed to {Config.RivalBrightness}");
						}
					}
					else
					{
						Logger.Warning("RIVAL OUTLINE BRIGHTNESS: Invalid floating point value");
					}
				},
				null,
				null,
				"ENTER OUTLINE BRIGHTNESS",
				null,
				"Change the brightness for Steam Rival car outlines." +
				" NOTE: Brightness values higher than 1.0 will only increase the intensity of flames and wing trails.")
				.WithDefaultValue(() => Config.RivalBrightness.ToString()); // Need to use this since there's no method for function defaults.

			return rivalsSubmenu;
		}

		#region Transpiler Helper Methods

		/// <summary>
		/// Gets the max number of replays that will automatically download when selecting a level.
		/// </summary>
		/// <remarks>
		/// This exists to inline a comparison operand.
		/// </remarks>
		public static int GetMaxAutoReplays()
		{
			return Math.Max(Math.Min(G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_, Mod.MaxReplaysAtAll), Mod.OriginalMaxReplays);
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
				return Math.Max(Math.Min(Mod.Instance.Config.MaxSelectedReplays, Mod.MaxReplaysAtAll), Mod.OriginalMaxReplays);
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
			return Math.Max(Math.Min(Mod.Instance.Config.MaxSavedLocalReplays, Mod.MaxSavedLocalReplaysAtAll), Mod.OriginalMaxSavedLocalReplays);
		}

		/// <summary>
		/// Gets the max number of entries that will show up in Friends/Global leaderboards.
		/// </summary>
		/// <remarks>
		/// This exists to inline a passed argument operand.
		/// </remarks>
		public static int GetMaxOnlineLeaderboards()
		{
			return Math.Max(Math.Min(Mod.Instance.Config.MaxOnlineLeaderboards, Mod.MaxOnlineLeaderboardsAtAll), Mod.MinOnlineLeaderboards);
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
