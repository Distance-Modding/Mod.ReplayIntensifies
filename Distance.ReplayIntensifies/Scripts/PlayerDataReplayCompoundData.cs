using System;
using UnityEngine;

namespace Distance.ReplayIntensifies.Scripts
{
	public class PlayerDataReplayCompoundData : MonoBehaviour
	{
		public PlayerDataReplay Player { get; internal set; }

		public bool OriginalIsGhost { get; internal set; }

		public CarLevelOfDetail.Type DetailType { get; internal set; }

		public bool HasOutline { get; internal set; }

		public bool IsRival { get; internal set; }

		/*public bool IsDefault
		{
			get
			{
				if (this.OriginalIsGhost)
				{
					return (this.DetailType == CarLevelOfDetail.Type.Ghost && this.HasOutline);
				}
				else
				{
					return (this.DetailType == CarLevelOfDetail.Type.Replay && !this.HasOutline);
				}
			}
		}*/

		public bool SimulateNetworkCar => this.DetailType == CarLevelOfDetail.Type.Networked;

		public bool IsGhost => this.DetailType == CarLevelOfDetail.Type.Ghost;

		// We want to calculate this property on the fly, since the ghost brightness setting can be changed mid-game.
		public float GetOutlineBrightness()
		{
			if (!this.HasOutline) // !this.OriginalIsGhost // !this.IsGhost
			{
				// For now treat 'no outline' as using the default brightness of 1.0,
				//  since this brightness is also applied for jet flames and wing trails.
				// TODO: Should replays just use the same outline brightness as ghosts??
				// TODO: If not, should brightness be decided based on being in Replay Mode vs. racing ghosts?
				return 1f;
			}
			else if (this.IsRival)
			{
				return Mod.Instance.Config.RivalBrightness;
			}
			else
			{
				return this.Player.replaySettings_.GhostBrightness_;
			}
		}
	}
}
