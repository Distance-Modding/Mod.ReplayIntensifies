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
	}
}
