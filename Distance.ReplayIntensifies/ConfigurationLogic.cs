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
			get => Get<CarLevelOfDetail.Level>(MaxDetailLevel_ID);
			set
			{
				Set(MaxDetailLevel_ID, value);
				this.MaxLevelOfDetailCached = value;
			}
		}

		private const string MinDetailLevel_ID = "visual.min_level_of_detail";
		/*public CarLevelOfDetail.Level MinLevelOfDetail
		{
			get => Get<CarLevelOfDetail.Level>(MinDetailLevel_ID);
			set
			{
				Set(MinDetailLevel_ID, value);
				this.MinLevelOfDetailCached = value;
			}
		}*/
		// NOTE: Min LOD has to be removed due to affecting the level environment.
		public CarLevelOfDetail.Level MinLevelOfDetail
		{
			get => CarLevelOfDetail.Level.Speck;
			set
			{
			}
		}

		private const string EnableUnrestrictedOpponentColors_ID = "visual.unrestricted_colors";
		public bool EnableUnrestrictedOpponentColors
		{
			get => Get<bool>(EnableUnrestrictedOpponentColors_ID);
			set => Set(EnableUnrestrictedOpponentColors_ID, value);
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

		#endregion

		#region Cached

		// Cached property values for faster accessing.
		public CarLevelOfDetail.Level MaxLevelOfDetailCached { get; private set; }
		//public CarLevelOfDetail.Level MinLevelOfDetailCached { get; private set; }
		// NOTE: Min LOD has to be removed due to affecting the level environment.
		public CarLevelOfDetail.Level MinLevelOfDetailCached
		{
			get => CarLevelOfDetail.Level.Speck;
			set
			{
			}
		}

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

		/*public void SetCarDetailType(bool isGhost, CarLevelOfDetail.Type value)
		{
			if (isGhost)
			{
				this.GhostDetailType = value;
			}
			else
			{
				this.ReplayDetailType = value;
			}
		}*/

		public bool GetCarOutline(bool isGhost, bool isCarRival)
		{
			if (isCarRival)
			{
				return this.RivalOutline;
			}
			return (isGhost) ? this.GhostOutline : this.ReplayOutline;
		}

		/*public void SetCarOutline(bool isGhost, bool value)
		{
			if (isGhost)
			{
				this.GhostOutline = value;
			}
			else
			{
				this.ReplayOutline = value;
			}
		}*/

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

			Get(MaxSavedLocalReplays_ID, 500);
			Get(MaxOnlineLeaderboards_ID, 1000);

			Get(GhostOutline_ID, true);
			Get(GhostDetailType_ID, CarLevelOfDetail.Type.Ghost);
			Get(ReplayOutline_ID, false);
			Get(ReplayDetailType_ID, CarLevelOfDetail.Type.Replay);
			this.MaxLevelOfDetailCached = Get(MaxDetailLevel_ID, CarLevelOfDetail.Level.InFocusFP);
			this.MinLevelOfDetailCached = Get(MinDetailLevel_ID, CarLevelOfDetail.Level.Speck);
			Get(EnableUnrestrictedOpponentColors_ID, false);

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
