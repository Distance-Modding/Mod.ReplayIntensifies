using UnityEngine;

namespace Distance.ReplayIntensifies.Scripts
{
	/// <summary>
	/// Storage for data that can only be determined before <see cref="PlayerDataReplayCompoundData"/>'s are created.
	/// </summary>
	public class CarReplayDataCompoundData : MonoBehaviour
	{
		// Replay is from online leaderboards.
		public bool IsOnline { get; internal set; }

		// Is this the replay created after finishing a run?
		public bool IsMyPlayer { get; internal set; }

		public bool DidNotFinish { get; internal set; }

		public int Placement { get; internal set; }
	}
}
