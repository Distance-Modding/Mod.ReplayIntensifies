using Reactor.API.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
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
			set => Set(MaxDetailLevel_ID, value);
		}

		private const string MinDetailLevel_ID = "visual.min_level_of_detail";
		public CarLevelOfDetail.Level MinLevelOfDetail
		{
			get => Get<CarLevelOfDetail.Level>(MinDetailLevel_ID);
			set => Set(MinDetailLevel_ID, value);
		}

		private const string EnableUnrestrictedOpponentColors_ID = "visual.unrestricted_colors";
		public bool EnableUnrestrictedOpponentColors
		{
			get => Get<bool>(EnableUnrestrictedOpponentColors_ID);
			set => Set(EnableUnrestrictedOpponentColors_ID, value);
		}

		#endregion

		#region Helpers

		public CarLevelOfDetail.Type GetCarDetailType(bool isGhost)
		{
			return (isGhost) ? this.GhostDetailType : this.ReplayDetailType;
		}

		public void SetCarDetailType(bool isGhost, CarLevelOfDetail.Type value)
		{
			if (isGhost)
			{
				this.GhostDetailType = value;
			}
			else
			{
				this.ReplayDetailType = value;
			}
		}

		public bool GetCarOutline(bool isGhost)
		{
			return (isGhost) ? this.GhostOutline : this.ReplayOutline;
		}

		public void SetCarOutline(bool isGhost, bool value)
		{
			if (isGhost)
			{
				this.GhostOutline = value;
			}
			else
			{
				this.ReplayOutline = value;
			}
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
			Get(MaxDetailLevel_ID, CarLevelOfDetail.Level.InFocusFP);
			Get(MinDetailLevel_ID, CarLevelOfDetail.Level.Speck);
			Get(EnableUnrestrictedOpponentColors_ID, false);

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

		public void Save()
		{
			Config?.Save();
			OnChanged?.Invoke(this);
		}
	}
}
