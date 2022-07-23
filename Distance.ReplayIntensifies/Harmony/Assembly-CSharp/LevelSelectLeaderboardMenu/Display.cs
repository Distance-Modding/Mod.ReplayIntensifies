using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Distance.ReplayIntensifies.Harmony
{
	/// <summary>
	/// Patch to add the ADD/REMOVE RIVAL(S) button to the bottom left button list when entering
	/// the level select leaderboards menu.
	/// </summary>
	/// <remarks>
	/// Required For: Add/Remove Rival(s) button.
	/// </remarks>
	[HarmonyPatch(typeof(LevelSelectLeaderboardMenu), nameof(LevelSelectLeaderboardMenu.Display))]
	internal static class LevelSelectLeaderboardMenu__Display
	{
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			Mod.Instance.Logger.Info("Transpiling...");
			// VISUAL:
			//menuPanel.Push();
			//this.ClearCategories();
			// -to-
			//AddMenuPanelButtons_(this);
			//menuPanel.Push();
			//this.ClearCategories();

			var codes = new List<CodeInstruction>(instructions);
			for (int i = 3; i < codes.Count; i++)
			{
				if ((codes[i - 2].opcode == OpCodes.Callvirt && ((MethodInfo)codes[i - 2].operand).Name == "Push") &&
					(codes[i    ].opcode == OpCodes.Call     && ((MethodInfo)codes[i    ].operand).Name == "ClearCategories"))
				{
					Mod.Instance.Logger.Info($"call MenuPanel.Push @ {i-2}");

					// Insert:  ldarg.0
					// Insert:  call AddMenuPanelButtons_
					// Before:  ldloc. (menuPanel)
					// Before:  callvirt MenuPanel.Push
					// NOTE: (i - 3) to insert before the ldloc used to call the MenuPanel.Push instance method.
					codes.InsertRange(i - 3, new CodeInstruction[]
					{
						new CodeInstruction(OpCodes.Ldarg_0, null),
						new CodeInstruction(OpCodes.Call, typeof(LevelSelectLeaderboardMenu__Display).GetMethod(nameof(AddMenuPanelButtons_))),
					});

					break;
				}
			}
			return codes.AsEnumerable();
		}

		#region Helper Functions

		public static void AddMenuPanelButtons_(LevelSelectLeaderboardMenu leaderboardMenu)
		{
			if (Mod.Instance.Config.EnableSteamRivals)
			{
				MenuPanel menuPanel = leaderboardMenu.GetComponent<MenuPanel>();
				menuPanel.SetBottomLeftButton(InputAction.MenuSpecial_3, "ADD RIVAL");
			}
		}

		#endregion
	}
}
