using Centrifuge.Distance.Game;
using Centrifuge.Distance.GUI.Data;
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

			Dictionary<string, CarLevelOfDetail.Level> detailLevelEntries = new Dictionary<string, CarLevelOfDetail.Level>
			{
				{ "In-Focus (First Person)", CarLevelOfDetail.Level.InFocusFP }, // 0
				{ "In-Focus",                CarLevelOfDetail.Level.InFocus },   // 1
				{ "Near",                    CarLevelOfDetail.Level.Near },      // 2
				{ "Medium",                  CarLevelOfDetail.Level.Medium },    // 3
				{ "Far",                     CarLevelOfDetail.Level.Far },       // 4
				{ "Very Far",                CarLevelOfDetail.Level.VeryFar },   // 5
				{ "Speck",                   CarLevelOfDetail.Level.Speck },     // 6
			};



			MenuTree settingsMenu = new MenuTree("menu.mod." + Mod.Name.ToLower(), Mod.FriendlyName);


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
				(value) => G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_ = value,
				0, Mod.MaxReplaysAtAll,
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
				detailLevelEntries,
				"Change the highest level of detail that opponent cars will render with." +
				" Lowering Max Level of Detail can improve performance when playing with more ghosts.");

			settingsMenu.ListBox<CarLevelOfDetail.Level>(MenuDisplayMode.Both,
				"setting:min_level_of_detail",
				"MIN CAR LEVEL OF DETAIL",
				() => Config.MinLevelOfDetail,
				(value) => Config.MinLevelOfDetail = value,
				detailLevelEntries,
				"Change the lowest level of detail that opponent cars will render with." +
				" Raising Min Level of Detail can decrease performance when playing with more ghosts.");


			settingsMenu.CheckBox(MenuDisplayMode.Both,
				"setting:enable_unrestricted_colors",
				"ENABLE UNRESTRICTED OPPONENT COLORS",
				() => Config.EnableUnrestrictedOpponentColors,
				(value) => Config.EnableUnrestrictedOpponentColors = value,
				"Online opponents and non-[i]Ghost Detail Type[/i] cars will NOT have their colors clamped, allowing for extremely bright cars." +
				" Bright cars are made by editing color preset files and changing the color channels to very large values.");


			Menus.AddNew(MenuDisplayMode.Both, settingsMenu,
				Mod.FriendlyName.ToUpper(),
				"Settings for replay limits, leaderboard limits, and car rendering.");
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
		public static bool GetClampCarColors(PlayerDataBase instance)
		{
			bool result = instance.ClampCarColors_;
			if (result && Mod.Instance.Config.EnableUnrestrictedOpponentColors)
			{
				if (instance is PlayerDataOpponent)
				{
					//Mod.Instance.Logger.Info("GetClampCarColors: instance is PlayerDataOpponent");
					return false;
				}
			}
			return result;
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

		#endregion
	}
}
