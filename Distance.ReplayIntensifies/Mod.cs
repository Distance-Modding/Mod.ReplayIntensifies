using Reactor.API.Attributes;
using Reactor.API.Interfaces.Systems;
using Reactor.API.Logging;
using Reactor.API.Runtime.Patching;
using System;
using UnityEngine;

namespace Distance.ReplayIntensifies
{
	/// <summary>
	/// The mod's main class containing its entry point
	/// </summary>
	[ModEntryPoint("com.github.reherc/ReplayIntensifies")]
	public sealed class Mod : MonoBehaviour
	{
		public const string Name = "ReplayIntensifies";
		public const string FullName = "Distance." + Name;
		public const string FriendlyName = "Replay Intensifies";

		public const int MaxReplaysAtAll = 1000;

		/// <summary>
		/// Static getter function for use as an instruction by Transpilers.
		/// </summary>
		/// <returns>The maximum replays, capped at either the set limit of max Arcade Ghosts, or 20.</returns>
		public static int GetMaxReplays()
		{
			return (int)Math.Max((int)Math.Min(Mod.MaxReplaysAtAll, G.Sys.OptionsManager_.Replay_.GhostsInArcadeCount_), 20);
		}


		public static Mod Instance { get; private set; }

		public IManager Manager { get; private set; }

		public Log Logger { get; private set; }

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

			try
			{
				RuntimePatcher.AutoPatch();
			}
			catch (Exception ex)
			{
				Logger.Error(Mod.Name + ": Error during RuntimePatcher.AutoPatch()");
				Mod.Instance.Logger.Exception(ex);
				throw;
			}

			Logger.Info(Mod.Name + ": Initialized!");
		}

	}
}



