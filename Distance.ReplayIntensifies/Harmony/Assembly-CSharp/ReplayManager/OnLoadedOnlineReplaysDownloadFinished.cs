using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to handle filling unused replay slots where there aren't enough online replays to load.
	/// <para/>
	/// This function is only called when auto-loading replays (AKA not when picking individual replays from a menu).
	/// </summary>
	/// <remarks>
	/// Required For: Fill With Local Replays.
	/// </remarks>
	[HarmonyPatch(typeof(ReplayManager), nameof(ReplayManager.OnLoadedOnlineReplaysDownloadFinished))]
	internal static class ReplayManager__OnLoadedOnlineReplaysDownloadFinished
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var codes = new List<CodeInstruction>(instructions);

			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			// After online replays are populated, handle filling unused slots with local replays.
			//this.AddOnlineReplaysToGroup(replays, this.loadedReplays_);
			// -to-
			//this.AddOnlineReplaysToGroup(replays, this.loadedReplays_);
			//FillRemainingWithLocalReplays_(this, this.loadedReplays_, userSkipped);

			for (int i = 1; i < codes.Count; i++)
			{
				if ((codes[i - 1].opcode == OpCodes.Ldfld) && // 'group' (should be loadedReplays_)
					(codes[i    ].opcode == OpCodes.Call && ((MethodInfo)codes[i    ].operand).Name == "AddOnlineReplaysToGroup"))
				{
					Mod.Instance.Logger.Info($"call AddOnlineReplaysToGroup @ {i}");

					// After:   call AddOnlineReplaysToGroup
					// Insert:  ldarg.0
					// Insert:  ldarg.0
					// Insert:  ldfld 'group' (should be loadedReplays_)
					// Insert:  ldarg.2
					// Insert:  call FillRemainingWithLocalReplays_
					codes.InsertRange(i + 1, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Ldfld, codes[i - 1].operand), // 'group' (should be loadedReplays_)
						new CodeInstruction(OpCodes.Ldarg_2, null), // userSkipped
						new CodeInstruction(OpCodes.Call, typeof(ReplayManager__OnLoadedOnlineReplaysDownloadFinished).GetMethod(nameof(FillRemainingWithLocalReplays_))),
					});

					break;
				}
			}

			return codes.AsEnumerable();
		}

		#region Helpers

		public static void FillRemainingWithLocalReplays_(ReplayManager replayManager, GameObject group, bool userSkipped)
		{
			if (!Mod.Instance.Config.FillWithLocalReplays)
			{
				return; // Stop now if the setting is disabled.
			}
			// TODO: Should we stop when the user skips loading the remaining online ghosts?
			//       This is assuming skip is generally only used when unintenionally started a level.
			/*if (userSkipped)
			{
				return;
			}*/

			CarReplayData[] onlineReplays = group.GetComponentsInChildren<CarReplayData>();
			int missingReplayCount = replayManager.settings_.GhostsInArcadeCount_ - onlineReplays.Length;
			if (missingReplayCount <= 0)
			{
				return; // We already have the maximum number of replays.
			}

			string levelPath = replayManager.gm_.NextLevelPath_;
			GameModeID gameModeID = replayManager.gm_.NextGameModeID_;
			LocalLeaderboard localLeaderboard = LocalLeaderboard.Load(levelPath, gameModeID);
			if (localLeaderboard != null)
			{
				// Depending on the player's ghost download type (Friends/Global Near Me/Global Best), it's possible that
				//  we've also downloaded one of our own ghosts. Because that ghost normally would be the top local replay
				//  on our leaderboard, we need to take time to filter that out to avoid duplicate overlapping ghosts.

				// NOTE: It's not guaranteed the top local replay will be our online ghost, since it's possible it was created
				//       while there was no online connection. So compare all local replays for `missingReplayCount + 1`.
				List<CarReplayData> localReplays = CarReplayData.LoadReplaysFromLeaderboard(localLeaderboard, missingReplayCount + 1).ToList();

				bool duplicateFound = false;
				for (int i = 0; i < onlineReplays.Length && !duplicateFound; i++)
				{
					var onlineReplay = onlineReplays[i];

					for (int j = 0; j < localReplays.Count; j++)
					{
						var localReplay = localReplays[j];
						// Compare the data that's most likely to differ first.
						if ((localReplay.steamID_ == 0 || localReplay.steamID_ == onlineReplay.steamID_) &&
							localReplay.FinishValue_        == onlineReplay.FinishValue_ &&
							localReplay.ReplayLengthMS_     == onlineReplay.ReplayLengthMS_ &&
							localReplay.StateBuffer_.Length == onlineReplay.StateBuffer_.Length &&
							localReplay.EventBuffer_.Length == onlineReplay.EventBuffer_.Length &&
							localReplay.carData_.name_              == onlineReplay.carData_.name_ &&
							localReplay.carData_.colors_.primary_   == onlineReplay.carData_.colors_.primary_ &&
							localReplay.carData_.colors_.secondary_ == onlineReplay.carData_.colors_.secondary_ &&
							localReplay.carData_.colors_.glow_      == onlineReplay.carData_.colors_.glow_ &&
							localReplay.carData_.colors_.sparkle_   == onlineReplay.carData_.colors_.sparkle_ &&
							localReplay.StateBuffer_.SequenceEqual(onlineReplay.StateBuffer_) &&
							localReplay.EventBuffer_.SequenceEqual(onlineReplay.EventBuffer_))
						{
							// This replay is the same as the player's online replay, avoid populating with duplicates.
							localReplays.RemoveAt(j);
							duplicateFound = true;
							break;
						}
					}
				}
				if (!duplicateFound && localReplays.Count > missingReplayCount)
				{
					// We obtained more replays than we wanted (to check for dups), so remove the last in the list.
					localReplays.RemoveAt(localReplays.Count - 1);
				}

				if (localReplays.Count > 0)
				{
					replayManager.AddReplaysToGroup(localReplays.ToArray(), replayManager.loadedReplays_);
				}
				UnityEngine.Object.Destroy(localLeaderboard.gameObject);
			}
		}

		#endregion
	}
}
