using Distance.ReplayIntensifies.Helpers;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to handle the logic/name/and enabled state for the ADD/REMOVE RIVAL(s) button in
	/// the level select leaderboards menu.
	/// </summary>
	/// <remarks>
	/// Required For: Add/Remove Rival(s) button.
	/// </remarks>
	[HarmonyPatch(typeof(LevelSelectLeaderboardMenu), nameof(LevelSelectLeaderboardMenu.Update))]
	internal static class LevelSelectLeaderboardMenu__Update
	{
		[HarmonyPostfix]
		internal static void Postfix(LevelSelectLeaderboardMenu __instance)
		{
			if (__instance.IsDownloading_)
			{
				return;
			}

			if (Mod.Instance.Config.EnableSteamRivals && G.Sys.MenuPanelManager_.IsTop(__instance.gameObject))
			{
				var picked = GetPickedSteamEntries_(__instance);
				// Remove self from list of picked entries (rival status is handled differently for that).
				picked.RemoveAll((entry) => SteamworksManager.GetSteamID() == entry.steamID);

				int numPicked = picked.Count;
				int numRivals = Mod.Instance.Config.CountSteamRivals(picked.Select((entry) => entry.steamID));

				// Enabled button if leaderboard list is non-empty and at least one entry that isn't the player is selected.
				bool buttonEnabled = picked.Count > 0;

				// Treat add/remove mode the same as Select All/Select None (if count < all, then add, otherwise remove)
				string text;
				bool removeMode;
				if (numRivals < numPicked || !buttonEnabled) // Second condition to treat "ADD RIVAL" as the default button text.
				{
					text = (numPicked > 1) ? "ADD RIVALS" : "ADD RIVAL";
					removeMode = false;
				}
				else
				{
					text = (numPicked > 1) ? "REMOVE RIVALS" : "REMOVE RIVAL";
					removeMode = true;
				}
				G.Sys.MenuPanelManager_.SetBottomLeftActionButton(InputAction.MenuSpecial_3, text);
				G.Sys.MenuPanelManager_.SetBottomLeftActionButtonEnabled(InputAction.MenuSpecial_3, buttonEnabled);

				// ==== UpdateInput() ====
				// Place our `UpdateInput` behavior in the same function, so that we don't need to recreate our picked lists.
				if (!G.Sys.MenuPanelManager_.MenuInputEnabled_ || G.Sys.GameManager_.IsFadingOut_)
				{
					return;
				}

				if (buttonEnabled && __instance.inputManager_.GetKeyUp(InputAction.MenuSpecial_3, -2))
				{
					int numChanged = 0;
					if (!removeMode)
					{
						numChanged = picked.Count((entry) => Mod.Instance.Config.AddSteamRival(entry.steamID, entry.entry.Name_, false));
					}
					else
					{
						numChanged = picked.Count((entry) => Mod.Instance.Config.RemoveSteamRival(entry.steamID, false));
					}

					if (numChanged > 0)
					{
						// Wait until the end to save, in-case we have multiple entries selected.
						Mod.Instance.Config.Save();

						// Update rival highlighting for buttons.
						foreach (var button in __instance.buttonList_.Buttons_)
						{
							if (button is LevelSelectLeaderboardButton leaderboardButton)
							{
								Mod.UpdateLeaderboardButtonColor(leaderboardButton, true);
							}
						}
					}
				}
			}
		}

		#region Helper Functions

		private struct PickedEntry
		{
			public readonly SteamworksLeaderboard.Entry entry;
			public readonly ulong steamID;

			public PickedEntry(SteamworksLeaderboard.Entry entry)
			{
				this.entry = entry;
				this.steamID = SteamworksHelper.GetLeaderboardEntrySteamID(entry);
			}
		}

		private static List<PickedEntry> GetPickedSteamEntries_(LevelSelectLeaderboardMenu leaderboardMenu)
		{
			var picked = leaderboardMenu.allEntries_.Select((entry) => entry.ReplayHandleIfToggled_ as SteamworksLeaderboard.Entry)
													.Where((steamEntry) => steamEntry != null)
													.Select((steamEntry) => new PickedEntry(steamEntry))
													.ToList();

			if (picked.Count == 0)
			{
				// If no toggled entries, then use the current highlighted entry.
				if ((leaderboardMenu.buttonList_.SelectedEntry_ is LevelSelectLeaderboardMenu.Entry entry) &&
					(entry.leaderboardEntry_ is SteamworksLeaderboard.Entry steamEntry))
				{
					picked.Add(new PickedEntry(steamEntry));
				}
			}
			return picked;
		}

		#endregion
	}
}
